using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Metrics;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Metrics;

namespace UniTrackRemaster.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    // [Authorize(Roles = "Admin,SuperAdmin")] // Restricting access to admin users
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;
        private readonly IMetricsService _metricsService;

        public MetricsController(
            ILogger<MetricsController> logger,
            IMetricsService metricsService)
        {
            _logger = logger;
            _metricsService = metricsService;
        }

        [HttpGet]
        [Route("application")]
        public async Task<ActionResult<ApplicationMetricsDto>> GetApplicationMetrics()
        {
            _logger.LogInformation("Getting application metrics");
            return Ok(await _metricsService.GetApplicationMetricsAsync());
        }

        [HttpGet]
        [Route("health")]
        public async Task<ActionResult<HealthMetricDto>> GetHealthMetrics()
        {
            _logger.LogInformation("Getting health metrics");
            return Ok(await _metricsService.GetHealthMetricsAsync());
        }

        [HttpGet]
        [Route("database")]
        public async Task<ActionResult<DatabaseMetricsDto>> GetDatabaseMetrics()
        {
            _logger.LogInformation("Getting database metrics");
            return Ok(await _metricsService.GetDatabaseMetricsAsync());
        }

        [HttpGet]
        [Route("requests")]
        public async Task<ActionResult<RequestMetricsDto>> GetRequestMetrics()
        {
            _logger.LogInformation("Getting request metrics");
            return Ok(await _metricsService.GetRequestMetricsAsync());
        }
    }
}
