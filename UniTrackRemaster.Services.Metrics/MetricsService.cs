using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Metrics;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Services.Metrics
{

    public class MetricsService : IMetricsService
    {
        private readonly ILogger<MetricsService> _logger;
        private readonly Meter _meter;
        private readonly Counter<long> _requestCounter;
        private readonly Counter<long> _errorCounter;
        private readonly Histogram<double> _requestDuration;
        private readonly Dictionary<string, int> _requestsByEndpoint = new();
        private readonly Dictionary<string, List<double>> _responseTimesByEndpoint = new();
        private readonly ActivitySource _activitySource;

        // For tracking active requests
        private int _activeRequests = 0;
        private readonly List<DateTimeOffset> _requestTimestamps = new();
        private readonly object _requestLock = new();

        // Internal counters for reading values
        private long _totalRequests = 0;
        private long _totalErrors = 0;

        private static readonly Process _currentProcess = Process.GetCurrentProcess();

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
            _meter = new Meter("UniTrack.Metrics", "1.0.0");
            _activitySource = new ActivitySource("UniTrack.Tracing", "1.0.0");

            _requestCounter = _meter.CreateCounter<long>("unitrack.requests.total", "Count", "Total number of requests");
            _errorCounter = _meter.CreateCounter<long>("unitrack.requests.errors", "Count", "Total number of error responses");
            _requestDuration = _meter.CreateHistogram<double>("unitrack.requests.duration", "ms", "Request duration in milliseconds");
        }

        public void RecordRequestStart()
        {
            lock (_requestLock)
            {
                _activeRequests++;
                _requestTimestamps.Add(DateTimeOffset.UtcNow);

                // Remove timestamps older than 1 minute to keep the list manageable
                var cutoff = DateTimeOffset.UtcNow.AddMinutes(-1);
                _requestTimestamps.RemoveAll(t => t < cutoff);
            }
        }

        public void RecordRequestEnd(string endpoint, double duration, bool isSuccess)
        {
            _requestCounter.Add(1);
            _requestDuration.Record(duration);

            // Update internal counters
            Interlocked.Increment(ref _totalRequests);

            if (!isSuccess)
            {
                _errorCounter.Add(1);
                Interlocked.Increment(ref _totalErrors);
            }

            lock (_requestsByEndpoint)
            {
                if (!_requestsByEndpoint.ContainsKey(endpoint))
                {
                    _requestsByEndpoint[endpoint] = 0;
                    _responseTimesByEndpoint[endpoint] = new List<double>();
                }

                _requestsByEndpoint[endpoint]++;
                _responseTimesByEndpoint[endpoint].Add(duration);
            }

            lock (_requestLock)
            {
                _activeRequests = Math.Max(0, _activeRequests - 1);
            }
        }

        public Task<ApplicationMetricsDto> GetApplicationMetricsAsync()
        {
            var metrics = new List<MetricDto>();

            // Basic metrics about the application
            metrics.Add(new MetricDto(
                "runtime.memory.usage",
                "Current memory usage of the application",
                "bytes",
                new List<MetricPointDto>
                {
                    new(
                        DateTimeOffset.UtcNow,
                        _currentProcess.WorkingSet64,
                        new Dictionary<string, string> { { "type", "working_set" } }
                    )
                }
            ));

            metrics.Add(new MetricDto(
                "runtime.cpu.usage",
                "CPU usage of the application",
                "percentage",
                new List<MetricPointDto>
                {
                    new(
                        DateTimeOffset.UtcNow,
                        _currentProcess.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount /
                            (DateTimeOffset.UtcNow - Process.GetCurrentProcess().StartTime).TotalMilliseconds * 100,
                        new Dictionary<string, string> { { "type", "total" } }
                    )
                }
            ));

            metrics.Add(new MetricDto(
                "runtime.threads",
                "Number of threads",
                "count",
                new List<MetricPointDto>
                {
                    new(
                        DateTimeOffset.UtcNow,
                        _currentProcess.Threads.Count,
                        new Dictionary<string, string> { { "type", "active" } }
                    )
                }
            ));

            return Task.FromResult(new ApplicationMetricsDto(
                DateTimeOffset.UtcNow,
                metrics
            ));
        }

        public Task<HealthMetricDto> GetHealthMetricsAsync()
        {
            double cpuUsage = _currentProcess.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount /
                (DateTimeOffset.UtcNow - Process.GetCurrentProcess().StartTime).TotalMilliseconds * 100;

            double memoryUsage = _currentProcess.WorkingSet64 / (1024.0 * 1024.0); // Convert to MB

            int activeRequests;
            double requestsPerSecond;
            double averageResponseTime;

            lock (_requestLock)
            {
                activeRequests = _activeRequests;

                // Calculate requests per second over the last minute
                var lastMinute = DateTimeOffset.UtcNow.AddSeconds(-60);
                int requestsLastMinute = _requestTimestamps.Count(t => t >= lastMinute);
                requestsPerSecond = requestsLastMinute / 60.0;
            }

            lock (_responseTimesByEndpoint)
            {
                var allResponseTimes = _responseTimesByEndpoint.Values.SelectMany(v => v).ToList();
                averageResponseTime = allResponseTimes.Count > 0 ? allResponseTimes.Average() : 0;
            }

            return Task.FromResult(new HealthMetricDto(
                cpuUsage,
                memoryUsage,
                activeRequests,
                requestsPerSecond,
                averageResponseTime
            ));
        }

        public Task<DatabaseMetricsDto> GetDatabaseMetricsAsync()
        {
            // In a real implementation, these would come from OpenTelemetry metrics collected from EF Core
            return Task.FromResult(new DatabaseMetricsDto(
                ActiveConnections: 0,
                MaxConnections: 100,
                AverageQueryTime: 0,
                QueryCount: 0,
                ErrorCount: 0
            ));
        }

        public Task<RequestMetricsDto> GetRequestMetricsAsync()
        {
            Dictionary<string, int> requestsByEndpoint;
            Dictionary<string, double> responseTimeByEndpoint = new();
            int totalRequests = (int)_totalRequests;
            int failedRequests = (int)_totalErrors;
            int successfulRequests = totalRequests - failedRequests;
            double averageResponseTime;

            lock (_requestsByEndpoint)
            {
                requestsByEndpoint = new Dictionary<string, int>(_requestsByEndpoint);

                foreach (var endpoint in _responseTimesByEndpoint.Keys)
                {
                    if (_responseTimesByEndpoint[endpoint].Count > 0)
                    {
                        responseTimeByEndpoint[endpoint] = _responseTimesByEndpoint[endpoint].Average();
                    }
                }

                var allResponseTimes = _responseTimesByEndpoint.Values.SelectMany(v => v).ToList();
                averageResponseTime = allResponseTimes.Count > 0 ? allResponseTimes.Average() : 0;
            }

            return Task.FromResult(new RequestMetricsDto(
                TotalRequests: totalRequests,
                SuccessfulRequests: successfulRequests,
                FailedRequests: failedRequests,
                AverageResponseTime: averageResponseTime,
                RequestsByEndpoint: requestsByEndpoint,
                ResponseTimeByEndpoint: responseTimeByEndpoint
            ));
        }
    }
}