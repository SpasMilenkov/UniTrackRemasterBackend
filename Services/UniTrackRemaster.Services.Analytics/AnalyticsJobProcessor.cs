using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Commons.Services;

namespace UniTrackRemaster.Services.Analytics;

public class AnalyticsJobProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AnalyticsJobProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(2); // Process jobs every 2 minutes

    public AnalyticsJobProcessor(IServiceProvider serviceProvider, ILogger<AnalyticsJobProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics Job Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobs(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing analytics jobs");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Analytics Job Processor stopped");
    }

    private async Task ProcessPendingJobs(CancellationToken cancellationToken)
    {
        List<Data.Models.Analytics.ReportGenerationJob> pendingJobs;
        
        // Get pending jobs in a separate scope
        using (var scope = _serviceProvider.CreateScope())
        {
            var reportService = scope.ServiceProvider.GetRequiredService<IAnalyticsReportService>();
            
            try
            {
                pendingJobs = await reportService.GetPendingJobsAsync();
                
                if (pendingJobs.Any())
                {
                    _logger.LogInformation("Found {JobCount} pending analytics jobs", pendingJobs.Count);
                }
                else
                {
                    _logger.LogDebug("No pending jobs found");
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching pending jobs");
                return;
            }
        }

        // Process jobs in parallel with proper scoping - each job gets its own scope
        var semaphore = new SemaphoreSlim(2, 2); // Max 2 concurrent jobs
        var tasks = pendingJobs.Select(job => ProcessSingleJobAsync(job.Id, semaphore, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task ProcessSingleJobAsync(Guid jobId, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        
        try
        {
            // Create a NEW scope for each job to avoid DbContext threading issues
            using var scope = _serviceProvider.CreateScope();
            var reportService = scope.ServiceProvider.GetRequiredService<IAnalyticsReportService>();
            
            _logger.LogInformation("Processing job {JobId}", jobId);
            
            // Process the job
            await reportService.ProcessReportGenerationJobAsync(jobId);
            
            _logger.LogInformation("Successfully completed job {JobId}", jobId);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Job processing {JobId} was cancelled", jobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process job {JobId}", jobId);
            
            // Try to mark the job as failed in a separate scope
            try
            {
                await MarkJobAsFailedAsync(jobId, ex.Message);
            }
            catch (Exception markFailedEx)
            {
                _logger.LogError(markFailedEx, "Failed to mark job {JobId} as failed", jobId);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task MarkJobAsFailedAsync(Guid jobId, string errorMessage)
    {
        using var scope = _serviceProvider.CreateScope();
        var reportService = scope.ServiceProvider.GetRequiredService<IAnalyticsReportService>();
        
        try
        {
            // We need to add this method to the service or handle it directly with DbContext
            var context = scope.ServiceProvider.GetRequiredService<Data.Context.UniTrackDbContext>();
            
            var job = await context.ReportGenerationJobs.FindAsync(jobId);
            if (job != null)
            {
                job.Status = Data.Models.Enums.JobStatus.Failed;
                job.ErrorMessage = errorMessage;
                job.RetryCount++;
                
                // Reschedule if under retry limit
                if (job.RetryCount < job.MaxRetries)
                {
                    job.Status = Data.Models.Enums.JobStatus.Pending;
                    job.ScheduledFor = DateTime.UtcNow.AddMinutes(Math.Pow(2, job.RetryCount)); // Exponential backoff
                    _logger.LogInformation("Rescheduling job {JobId} for retry {RetryCount}/{MaxRetries}", 
                        jobId, job.RetryCount, job.MaxRetries);
                }
                
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update job status for {JobId}", jobId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Analytics Job Processor is stopping...");
        await base.StopAsync(cancellationToken);
    }
}