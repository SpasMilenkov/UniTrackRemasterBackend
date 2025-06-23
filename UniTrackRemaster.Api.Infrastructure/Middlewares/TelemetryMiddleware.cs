using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Services.Metrics;

namespace Infrastructure.Middlewares
{
    public class MetricsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MetricsMiddleware> _logger;
        private readonly MetricsService _metricsService;

        public MetricsMiddleware(
            RequestDelegate next,
            ILogger<MetricsMiddleware> logger,
            MetricsService metricsService)
        {
            _next = next;
            _logger = logger;
            _metricsService = metricsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Record request start
            _metricsService.RecordRequestStart();

            var sw = Stopwatch.StartNew();
            bool isSuccess = true;

            try
            {
                await _next(context);

                // Consider non-200 status codes as errors
                if (context.Response.StatusCode >= 400)
                {
                    isSuccess = false;
                }
            }
            catch (Exception)
            {
                isSuccess = false;
                    throw;
            }
            finally
            {
                sw.Stop();
                var endpoint = $"{context.Request.Method} {context.Request.Path}";
                _metricsService.RecordRequestEnd(endpoint, sw.Elapsed.TotalMilliseconds, isSuccess);

                _logger.LogDebug(
                    "Request {Method} {Path} completed in {ElapsedMilliseconds}ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
        }
    }
}