using UniTrackRemaster.Api.Dto.Metrics;

namespace UniTrackRemaster.Commons.Services;

public interface IMetricsService
{
    Task<ApplicationMetricsDto> GetApplicationMetricsAsync();
    Task<HealthMetricDto> GetHealthMetricsAsync();
    Task<DatabaseMetricsDto> GetDatabaseMetricsAsync();
    Task<RequestMetricsDto> GetRequestMetricsAsync();
}