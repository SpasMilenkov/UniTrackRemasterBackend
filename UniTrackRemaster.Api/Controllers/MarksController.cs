using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Mark;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.Academics;

namespace UniTrackRemaster.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarksController : ControllerBase
{
    private readonly IMarkService _markService;

    public MarksController(IMarkService markService)
    {
        _markService = markService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MarkResponseDto>>> GetAllMarks(CancellationToken cancellationToken = default)
    {
        var marks = await _markService.GetAllMarksAsync(cancellationToken);
        return Ok(marks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MarkResponseDto>> GetMarkById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var mark = await _markService.GetMarkByIdAsync(id, cancellationToken);
            return Ok(mark);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<IEnumerable<MarkResponseDto>>> GetMarksByStudentId(Guid studentId, CancellationToken cancellationToken = default)
    {
        var marks = await _markService.GetMarksByStudentIdAsync(studentId);
        return Ok(marks);
    }

    [HttpGet("teacher/{teacherId}")]
    public async Task<ActionResult<IEnumerable<MarkResponseDto>>> GetMarksByTeacherId(Guid teacherId, CancellationToken cancellationToken = default)
    {
        var marks = await _markService.GetMarksByTeacherIdAsync(teacherId);
        return Ok(marks);
    }

    [HttpGet("subject/{subjectId}")]
    public async Task<ActionResult<IEnumerable<MarkResponseDto>>> GetMarksBySubjectId(Guid subjectId, CancellationToken cancellationToken = default)
    {
        var marks = await _markService.GetMarksBySubjectIdAsync(subjectId);
        return Ok(marks);
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<MarkResponseDto>>> GetMarksByFilter(
        [FromQuery] Guid? studentId, 
        [FromQuery] Guid? teacherId, 
        [FromQuery] Guid? subjectId, 
        [FromQuery] MarkType? type,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var filterParams = new MarkFilterParams
        {
            StudentId = studentId,
            TeacherId = teacherId,
            SubjectId = subjectId,
            Type = type,
            FromDate = fromDate,
            ToDate = toDate
        };
        
        var marks = await _markService.GetMarksByFilterAsync(filterParams, cancellationToken);
        return Ok(marks);
    }

    [HttpPost]
    public async Task<ActionResult<MarkResponseDto>> CreateMark(CreateMarkDto createMarkDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var mark = await _markService.CreateMarkAsync(createMarkDto, cancellationToken);
            return CreatedAtAction(nameof(GetMarkById), new { id = mark.Id }, mark);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MarkResponseDto>> UpdateMark(Guid id, UpdateMarkDto updateMarkDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var mark = await _markService.UpdateMarkAsync(id, updateMarkDto, cancellationToken);
            return Ok(mark);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMark(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _markService.DeleteMarkAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // Statistics endpoints
    [HttpGet("statistics/student/{studentId}/average")]
    public async Task<ActionResult<decimal>> GetAverageMarkForStudent(
        Guid studentId, 
        [FromQuery] Guid? subjectId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var average = await _markService.GetAverageMarkForStudentAsync(studentId, subjectId);
            return Ok(average);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("statistics/subject/{subjectId}/average")]
    public async Task<ActionResult<decimal>> GetAverageMarkForSubject(Guid subjectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var average = await _markService.GetAverageMarkForSubjectAsync(subjectId);
            return Ok(average);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}