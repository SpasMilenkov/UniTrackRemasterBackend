using UniTrackRemaster.Api.Dto.Metrics;

namespace UniTrackRemaster.Commons.Services;

public interface IEntityStatisticsService
{
    Task<UserStatisticsDto> GetUserStatisticsAsync(CancellationToken cancellationToken = default);
    Task<AcademicStatisticsDto> GetAcademicStatisticsAsync(CancellationToken cancellationToken = default);
    Task<ActivityStatisticsDto> GetActivityStatisticsAsync(CancellationToken cancellationToken = default);
    Task<SystemStatisticsDto> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);
    Task<EntityCountsDto> GetAllEntityCountsAsync(CancellationToken cancellationToken = default);
}