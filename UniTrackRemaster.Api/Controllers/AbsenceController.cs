using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Absence;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;

/// <summary>
/// Controller for managing absence records with semester support.
/// Provides endpoints for CRUD operations and attendance statistics.
/// </summary>
[ApiController]
[Route("api/absences")]
[Authorize]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AbsenceController : ControllerBase
{
    private readonly IAbsenceService _absenceService;
    private readonly ILogger<AbsenceController> _logger;

    /// <summary>
    /// Initializes a new instance of the AbsenceController.
    /// </summary>
    /// <param name="absenceService">Service for absence operations</param>
    /// <param name="logger">Logger instance</param>
    public AbsenceController(IAbsenceService absenceService, ILogger<AbsenceController> logger)
    {
        _absenceService = absenceService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a specific absence record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the absence record</param>
    /// <returns>The absence details if found</returns>
    /// <response code="200">Returns the absence record</response>
    /// <response code="404">If the absence record is not found</response>
    /// <response code="500">If an error occurs during retrieval</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AbsenceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AbsenceResponseDto>> GetById(Guid id)
    {
        try
        {
            var absence = await _absenceService.GetByIdAsync(id);
            return Ok(absence);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving absence with ID {AbsenceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the absence");
        }
    }

    /// <summary>
    /// Retrieves all absence records for a specific student with optional semester filtering.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to all semesters if not specified)</param>
    /// <returns>A list of absence records for the student</returns>
    /// <response code="200">Returns the list of absence records</response>
    /// <response code="404">If the student is not found</response>
    /// <response code="500">If an error occurs during retrieval</response>
    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AbsenceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AbsenceResponseDto>>> GetByStudent(
        Guid studentId,
        [FromQuery] Guid? semesterId = null)
    {
        try
        {
            var absences = await _absenceService.GetByStudentIdAsync(studentId, semesterId);
            return Ok(absences);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving absences for student {StudentId} in semester {SemesterId}", 
                studentId, semesterId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving student absences");
        }
    }

    /// <summary>
    /// Retrieves all absence records for a specific subject with optional semester filtering.
    /// </summary>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester if not specified)</param>
    /// <returns>A list of absence records for the subject</returns>
    /// <response code="200">Returns the list of absence records</response>
    /// <response code="404">If the subject is not found</response>
    /// <response code="500">If an error occurs during retrieval</response>
    [HttpGet("subject/{subjectId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AbsenceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AbsenceResponseDto>>> GetBySubject(
        Guid subjectId,
        [FromQuery] Guid? semesterId = null)
    {
        try
        {
            var absences = await _absenceService.GetBySubjectAsync(subjectId, semesterId);
            return Ok(absences);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving absences for subject {SubjectId} in semester {SemesterId}", 
                subjectId, semesterId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving subject absences");
        }
    }

    /// <summary>
    /// Retrieves absence records for a student in a specific subject with optional semester filtering.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>A list of absence records for the student in the specified subject</returns>
    /// <response code="200">Returns the list of absence records</response>
    /// <response code="404">If the student or subject is not found</response>
    /// <response code="500">If an error occurs during retrieval</response>
    [HttpGet("student/{studentId:guid}/subject/{subjectId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<AbsenceResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<AbsenceResponseDto>>> GetByStudentAndSubject(
        Guid studentId,
        Guid subjectId,
        [FromQuery] Guid? semesterId = null)
    {
        try
        {
            var absences = await _absenceService.GetByStudentAndSubjectAsync(studentId, subjectId, semesterId);
            return Ok(absences);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving absences for student {StudentId} in subject {SubjectId} and semester {SemesterId}", 
                studentId, subjectId, semesterId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while retrieving student absences for the subject");
        }
    }

    /// <summary>
    /// Retrieves attendance statistics for a specific student with optional semester filtering.
    /// </summary>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>Attendance statistics including rates, totals, and breakdown</returns>
    /// <response code="200">Returns the attendance statistics</response>
    /// <response code="404">If the student is not found</response>
    /// <response code="500">If an error occurs during calculation</response>
    [HttpGet("student/{studentId:guid}/statistics")]
    [ProducesResponseType(typeof(StudentAttendanceStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StudentAttendanceStatsDto>> GetStudentAttendanceStats(
        Guid studentId,
        [FromQuery] Guid? semesterId = null)
    {
        try
        {
            var stats = await _absenceService.GetStudentAttendanceStatsAsync(studentId, semesterId);
            return Ok(stats);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance statistics for student {StudentId} in semester {SemesterId}", 
                studentId, semesterId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while calculating attendance statistics");
        }
    }

    /// <summary>
    /// Retrieves attendance summary for a specific subject with optional semester filtering.
    /// </summary>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>Subject attendance summary including student counts and absence statistics</returns>
    /// <response code="200">Returns the subject attendance summary</response>
    /// <response code="404">If the subject is not found</response>
    /// <response code="500">If an error occurs during calculation</response>
    [HttpGet("subject/{subjectId:guid}/summary")]
    [ProducesResponseType(typeof(SubjectAttendanceSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubjectAttendanceSummaryDto>> GetSubjectAttendanceSummary(
        Guid subjectId,
        [FromQuery] Guid? semesterId = null)
    {
        try
        {
            var summary = await _absenceService.GetSubjectAttendanceSummaryAsync(subjectId, semesterId);
            return Ok(summary);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance summary for subject {SubjectId} in semester {SemesterId}", 
                subjectId, semesterId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "An error occurred while calculating subject attendance summary");
        }
    }

    /// <summary>
    /// Creates a new absence record with automatic semester detection.
    /// If no semester is specified, the system will use the current active semester.
    /// </summary>
    /// <param name="dto">The absence creation data</param>
    /// <returns>The created absence record</returns>
    /// <response code="201">Returns the newly created absence record</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If referenced entities (student, teacher, subject) are not found</response>
    /// <response code="500">If an error occurs during creation</response>
    [HttpPost]
    [ProducesResponseType(typeof(AbsenceResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AbsenceResponseDto>> Create([FromBody] CreateAbsenceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var absence = await _absenceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = absence.Id }, absence);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating absence record for student {StudentId}", dto.StudentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the absence record");
        }
    }

    /// <summary>
    /// Updates an existing absence record.
    /// </summary>
    /// <param name="id">The unique identifier of the absence record to update</param>
    /// <param name="dto">The updated absence data</param>
    /// <returns>The updated absence record</returns>
    /// <response code="200">Returns the updated absence record</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the absence record is not found</response>
    /// <response code="500">If an error occurs during update</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AbsenceResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AbsenceResponseDto>> Update(Guid id, [FromBody] UpdateAbsenceDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var absence = await _absenceService.UpdateAsync(id, dto);
            return Ok(absence);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating absence with ID {AbsenceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the absence record");
        }
    }

    /// <summary>
    /// Deletes an absence record from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the absence record to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Absence record successfully deleted</response>
    /// <response code="404">If the absence record is not found</response>
    /// <response code="500">If an error occurs during deletion</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            await _absenceService.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting absence with ID {AbsenceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the absence record");
        }
    }
}