using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Metrics;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Metrics;

namespace UniTrackRemaster.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    // [Authorize(Roles = "Admin,SuperAdmin")] // Restricting access to admin users
    public class StatisticsController : ControllerBase
    {
        private readonly ILogger<StatisticsController> _logger;
        private readonly IEntityStatisticsService _entityStatisticsService;

        public StatisticsController(
            ILogger<StatisticsController> logger,
            IEntityStatisticsService entityStatisticsService)
        {
            _logger = logger;
            _entityStatisticsService = entityStatisticsService;
        }

        [HttpGet("users")]
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting user statistics");
            var statistics = await _entityStatisticsService.GetUserStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }

        [HttpGet("academic")]
        public async Task<ActionResult<AcademicStatisticsDto>> GetAcademicStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting academic statistics");
            var statistics = await _entityStatisticsService.GetAcademicStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }

        [HttpGet("activity")]
        public async Task<ActionResult<ActivityStatisticsDto>> GetActivityStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting activity statistics");
            var statistics = await _entityStatisticsService.GetActivityStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }

        [HttpGet("system")]
        public async Task<ActionResult<SystemStatisticsDto>> GetSystemStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting system statistics");
            var statistics = await _entityStatisticsService.GetSystemStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }

        [HttpGet("entities")]
        public async Task<ActionResult<EntityCountsDto>> GetEntityCounts(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting entity counts");
            var counts = await _entityStatisticsService.GetAllEntityCountsAsync(cancellationToken);
            return Ok(counts);
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboardStatistics(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting dashboard statistics");

            // Get all statistics in parallel
            var systemStatsTask = await _entityStatisticsService.GetSystemStatisticsAsync(cancellationToken);
            var entityCountsTask = await _entityStatisticsService.GetAllEntityCountsAsync(cancellationToken);

            // Combine results into a dashboard view
            var dashboard = new
            {
                System = systemStatsTask,
                Entities = entityCountsTask,
                LastUpdated = DateTimeOffset.UtcNow
            };

            return Ok(dashboard);
        }
    }
}
