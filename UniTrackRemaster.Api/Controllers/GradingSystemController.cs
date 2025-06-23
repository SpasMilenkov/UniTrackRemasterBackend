using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Grading;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Organization;

namespace UniTrackRemaster.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GradingSystemsController : ControllerBase
    {
        private readonly IGradingSystemService _gradingSystemService;

        public GradingSystemsController(IGradingSystemService gradingSystemService)
        {
            _gradingSystemService = gradingSystemService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<GradingSystemResponseDto>>> GetAll(CancellationToken cancellationToken = default)
        {
            var gradingSystems = await _gradingSystemService.GetAllAsync(cancellationToken);
            return Ok(gradingSystems);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GradingSystemResponseDto>> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var gradingSystem = await _gradingSystemService.GetByIdAsync(id, cancellationToken);
                return Ok(gradingSystem);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Grading system with ID {id} not found");
            }
        }

        [HttpGet("institution/{institutionId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<GradingSystemResponseDto>>> GetAllForInstitution(Guid institutionId, CancellationToken cancellationToken = default)
        {
            var gradingSystems = await _gradingSystemService.GetAllForInstitutionAsync(institutionId, cancellationToken);
            return Ok(gradingSystems);
        }

        [HttpGet("institution/{institutionId:guid}/default")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<GradingSystemResponseDto>> GetDefaultForInstitution(Guid institutionId, CancellationToken cancellationToken = default)
        {
            try
            {
                var gradingSystem = await _gradingSystemService.GetDefaultForInstitutionAsync(institutionId, cancellationToken);
                return Ok(gradingSystem);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No default grading system found for institution {institutionId}");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,SuperAdmin,InstitutionAdmin")]
        public async Task<ActionResult<GradingSystemResponseDto>> Create([FromBody] CreateGradingSystemDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var gradingSystem = await _gradingSystemService.CreateAsync(dto, cancellationToken);
                return CreatedAtAction(nameof(GetById), new { id = gradingSystem.Id }, gradingSystem);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,SuperAdmin,InstitutionAdmin")]
        public async Task<ActionResult<GradingSystemResponseDto>> Update(Guid id, [FromBody] UpdateGradingSystemDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var gradingSystem = await _gradingSystemService.UpdateAsync(id, dto, cancellationToken);
                return Ok(gradingSystem);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Grading system with ID {id} not found");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,SuperAdmin,InstitutionAdmin")]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _gradingSystemService.DeleteAsync(id, cancellationToken);
                if (!result)
                {
                    return NotFound($"Grading system with ID {id} not found");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}/set-default/institution/{institutionId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,SuperAdmin,InstitutionAdmin")]
        public async Task<ActionResult> SetDefault(Guid id, Guid institutionId, CancellationToken cancellationToken = default)
        {
            var result = await _gradingSystemService.SetDefaultAsync(id, institutionId, cancellationToken);
            if (!result)
            {
                return NotFound($"Grading system with ID {id} not found or not associated with institution {institutionId}");
            }

            return Ok();
        }

        [HttpPost("institution/{institutionId:guid}/initialize")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Admin,SuperAdmin,InstitutionAdmin")]
        public async Task<ActionResult> InitializeDefaultGradingSystems(Guid institutionId, CancellationToken cancellationToken = default)
        {
            var result = await _gradingSystemService.InitializeDefaultGradingSystemsAsync(institutionId, cancellationToken);
            if (!result)
            {
                return BadRequest("Grading systems already exist for this institution");
            }

            return Ok("Default grading systems initialized successfully");
        }

        [HttpGet("convert/score-to-grade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> ConvertScoreToGrade(decimal score, Guid gradingSystemId)
        {
            try
            {
                var grade = _gradingSystemService.ConvertScoreToGrade(score, gradingSystemId);
                return Ok(grade);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("convert/score-to-gpa")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<double>> ConvertScoreToGpaPoints(decimal score, Guid gradingSystemId)
        {
            try
            {
                var gpaPoints = _gradingSystemService.ConvertScoreToGpaPoints(score, gradingSystemId);
                return Ok(gpaPoints);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("determine/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<string>> DetermineStatus(decimal score, Guid gradingSystemId)
        {
            try
            {
                var status = _gradingSystemService.DetermineStatus(score, gradingSystemId);
                return Ok(status);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("convert/grade-to-score")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<decimal>> ConvertGradeToScore(string grade, Guid gradingSystemId)
        {
            try
            {
                var score = _gradingSystemService.ConvertGradeToScore(grade, gradingSystemId);
                return Ok(score);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
