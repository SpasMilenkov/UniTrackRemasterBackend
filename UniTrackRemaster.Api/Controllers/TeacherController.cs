using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Absence;
using UniTrackRemaster.Api.Dto.Mark;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Subject;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Api.Dto.Teacher.Analytics;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Services.Academics;
using UniTrackRemaster.Services.User.Students;
using UniTrackRemaster.Services.User.Teachers;

namespace UniTrackRemaster.Controllers;


/// <summary>
/// Controller for managing teacher-related operations including academic activities, analytics, and grade assignments.
/// Provides endpoints for teacher CRUD operations, mark management, absence tracking, and comprehensive analytics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(401)]
[ProducesResponseType(403)]
public class TeachersController : ControllerBase
{
    private readonly ITeacherService _teacherService;
    private readonly ISubjectService _subjectService;
    private readonly IMarkService _markService;
    private readonly IAbsenceService _absenceService;
    private readonly IStudentService _studentService;
    private readonly ILogger<TeachersController> _logger;

    /// <summary>
    /// Initializes a new instance of the TeachersController.
    /// </summary>
    /// <param name="teacherService">Service for teacher operations</param>
    /// <param name="subjectService">Service for subject operations</param>
    /// <param name="markService">Service for mark operations</param>
    /// <param name="absenceService">Service for absence operations</param>
    /// <param name="studentService">Service for student operations</param>
    /// <param name="logger">Logger instance</param>
    public TeachersController(
        ITeacherService teacherService,
        ISubjectService subjectService,
        IMarkService markService,
        IAbsenceService absenceService,
        IStudentService studentService,
        ILogger<TeachersController> logger)
    {
        _teacherService = teacherService;
        _subjectService = subjectService;
        _markService = markService;
        _absenceService = absenceService;
        _studentService = studentService;
        _logger = logger;
    }

    #region Teacher Management

    /// <summary>
    /// Retrieves a teacher by their user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the user associated with the teacher</param>
    /// <returns>The teacher information if found</returns>
    /// <response code="200">Returns the teacher information</response>
    /// <response code="404">If no teacher is found for the given user ID</response>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin,Teacher")]
    [ProducesResponseType(typeof(TeacherResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TeacherResponseDto>> GetByUserId(Guid userId)
    {
        var teacher = await _teacherService.GetByUserIdAsync(userId);
        if (teacher == null)
            return NotFound("Teacher not found for the given user ID");

        return Ok(teacher);
    }

    /// <summary>
    /// Retrieves all teachers for a specific institution.
    /// </summary>
    /// <param name="institutionId">The unique identifier of the institution</param>
    /// <returns>A list of teachers in the institution</returns>
    /// <response code="200">Returns the list of teachers</response>
    /// <response code="404">If no teachers are found for the institution</response>
    [HttpGet("institution/{institutionId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetByInstitutionId(Guid institutionId)
    {
        var teachers = await _teacherService.GetByInstitutionIdAsync(institutionId);
        if (teachers == null || !teachers.Any())
            return NotFound("No teachers found for the given institution ID");

        return Ok(teachers);
    }

    /// <summary>
    /// Retrieves a specific teacher by their ID.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher</param>
    /// <returns>The teacher information</returns>
    /// <response code="200">Returns the teacher information</response>
    /// <response code="404">If the teacher is not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeacherResponseDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TeacherResponseDto>> GetById(Guid id)
    {
        return await _teacherService.GetByIdAsync(id);
    }

    /// <summary>
    /// Retrieves all teachers in the system.
    /// </summary>
    /// <returns>A list of all teachers</returns>
    /// <response code="200">Returns the list of all teachers</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), 200)]
    public async Task<ActionResult<IEnumerable<TeacherResponseDto>>> GetAll()
    {
        return Ok(await _teacherService.GetAllAsync());
    }

    /// <summary>
    /// Creates a new teacher invitation.
    /// </summary>
    /// <param name="dto">The teacher creation data</param>
    /// <returns>The created teacher information</returns>
    /// <response code="201">Returns the newly created teacher</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If referenced entities (user, institution, grade) are not found</response>
    /// <response code="409">If a teacher profile already exists for the user</response>
    [HttpPost]
    [ProducesResponseType(typeof(TeacherResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<ActionResult<TeacherResponseDto>> Create(CreateTeacherDto dto)
    {
        var teacher = await _teacherService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacher);
    }

    /// <summary>
    /// Updates an existing teacher's information.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher to update</param>
    /// <param name="dto">The updated teacher information</param>
    /// <returns>The updated teacher information</returns>
    /// <response code="200">Returns the updated teacher information</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="404">If the teacher is not found</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TeacherResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TeacherResponseDto>> Update(Guid id, UpdateTeacherDto dto)
    {
        return await _teacherService.UpdateAsync(id, dto);
    }

    /// <summary>
    /// Deletes a teacher from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the teacher to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Teacher successfully deleted</response>
    /// <response code="404">If the teacher is not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _teacherService.DeleteAsync(id);
        return NoContent();
    }

    #endregion

    #region Subject and Student Management

    /// <summary>
    /// Retrieves all subjects taught by a specific teacher.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <returns>A list of subjects taught by the teacher</returns>
    /// <response code="200">Returns the list of subjects</response>
    /// <response code="404">If the teacher is not found</response>
    [HttpGet("{teacherId:guid}/subjects")]
    [ProducesResponseType(typeof(IEnumerable<SubjectResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<SubjectResponseDto>>> GetTeacherSubjects(Guid teacherId)
    {
        var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
        return Ok(subjects);
    }

    /// <summary>
    /// Retrieves students in a specific subject taught by the teacher, grouped by grade.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <returns>Students grouped by grade for the specified subject</returns>
    /// <response code="200">Returns the students grouped by grade</response>
    /// <response code="404">If the teacher or subject is not found</response>
    /// <response code="403">If the teacher is not authorized to view this subject</response>
    [HttpGet("{teacherId:guid}/subjects/{subjectId:guid}/students")]
    [ProducesResponseType(typeof(IEnumerable<StudentsByGradeDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<StudentsByGradeDto>>> GetStudentsBySubjectAndGrade(
        Guid teacherId, Guid subjectId)
    {
        var students = await _teacherService.GetStudentsBySubjectAndGradeAsync(teacherId, subjectId);
        return Ok(students);
    }

    #endregion

    #region Mark Management

    /// <summary>
    /// Creates a new mark for a student with automatic grading system conversion.
    /// The grade input will be converted to a 0-100 score using the institution's grading system.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher creating the mark</param>
    /// <param name="dto">The mark creation data including grade or score</param>
    /// <returns>The created mark with grading system enhancements</returns>
    /// <response code="201">Returns the newly created mark</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="403">If the teacher is not authorized to assign marks for this subject</response>
    /// <response code="404">If referenced entities are not found</response>
    [HttpPost("{teacherId:guid}/marks")]
    [ProducesResponseType(typeof(MarkResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<MarkResponseDto>> CreateMark(
        Guid teacherId, CreateMarkDto dto)
    {
        // Validate that the teacher is teaching the subject
        var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
        if (!subjects.Any(s => s.Id == dto.SubjectId))
            return Forbid("You are not authorized to assign marks for this subject");

        if (dto.TeacherId != teacherId) 
            return Forbid("Teacher ID mismatch");

        var mark = await _markService.CreateMarkAsync(dto);
        return CreatedAtAction(nameof(GetMarkById), new { teacherId, markId = mark.Id }, mark);
    }

    /// <summary>
    /// Retrieves a specific mark by ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="markId">The unique identifier of the mark</param>
    /// <returns>The mark information with grading system enhancements</returns>
    /// <response code="200">Returns the mark information</response>
    /// <response code="403">If the mark doesn't belong to this teacher</response>
    /// <response code="404">If the mark is not found</response>
    [HttpGet("{teacherId:guid}/marks/{markId:guid}")]
    [ProducesResponseType(typeof(MarkResponseDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<MarkResponseDto>> GetMarkById(Guid teacherId, Guid markId)
    {
        var mark = await _markService.GetMarkByIdAsync(markId);

        // Ensure the mark belongs to this teacher
        if (mark.Teacher.Id != teacherId) 
            return Forbid();

        return Ok(mark);
    }

    /// <summary>
    /// Updates an existing mark with grading system conversion.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="markId">The unique identifier of the mark to update</param>
    /// <param name="dto">The updated mark information</param>
    /// <returns>The updated mark with grading system enhancements</returns>
    /// <response code="200">Returns the updated mark</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="403">If the mark doesn't belong to this teacher</response>
    /// <response code="404">If the mark is not found</response>
    [HttpPut("{teacherId:guid}/marks/{markId:guid}")]
    [ProducesResponseType(typeof(MarkResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<MarkResponseDto>> UpdateMark(
        Guid teacherId, Guid markId, UpdateMarkDto dto)
    {
        var mark = await _markService.GetMarkByIdAsync(markId);

        // Ensure the mark belongs to this teacher
        if (mark.Teacher.Id != teacherId) 
            return Forbid();

        return await _markService.UpdateMarkAsync(markId, dto);
    }

    /// <summary>
    /// Deletes a mark from the system.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="markId">The unique identifier of the mark to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Mark successfully deleted</response>
    /// <response code="403">If the mark doesn't belong to this teacher</response>
    /// <response code="404">If the mark is not found</response>
    [HttpDelete("{teacherId:guid}/marks/{markId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteMark(Guid teacherId, Guid markId)
    {
        var mark = await _markService.GetMarkByIdAsync(markId);

        // Ensure the mark belongs to this teacher
        if (mark.Teacher.Id != teacherId) 
            return Forbid();

        await _markService.DeleteMarkAsync(markId);
        return NoContent();
    }

    /// <summary>
    /// Retrieves all marks given by a teacher, with optional semester filtering.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>A list of marks given by the teacher</returns>
    /// <response code="200">Returns the list of marks</response>
    /// <response code="404">If the teacher is not found</response>
    [HttpGet("{teacherId:guid}/marks")]
    [ProducesResponseType(typeof(IEnumerable<MarkResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<MarkResponseDto>>> GetTeacherMarks(
        Guid teacherId, 
        [FromQuery] Guid? semesterId = null)
    {
        var marks = await _markService.GetMarksByTeacherIdAsync(teacherId, semesterId);
        return Ok(marks);
    }

    /// <summary>
    /// Retrieves marks for a specific student in a specific subject.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>A list of marks for the student in the subject</returns>
    /// <response code="200">Returns the list of marks</response>
    /// <response code="403">If the teacher is not authorized to view marks for this subject</response>
    /// <response code="404">If entities are not found</response>
    [HttpGet("{teacherId:guid}/subjects/{subjectId:guid}/students/{studentId:guid}/marks")]
    [ProducesResponseType(typeof(IEnumerable<MarkResponseDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<MarkResponseDto>>> GetStudentMarksBySubject(
        Guid teacherId, Guid subjectId, Guid studentId, [FromQuery] Guid? semesterId = null)
    {
        var filterParams = new MarkFilterParams
        {
            StudentId = studentId,
            SubjectId = subjectId,
            TeacherId = teacherId,
            SemesterId = semesterId
        };

        var marks = await _markService.GetMarksByFilterAsync(filterParams);
        return Ok(marks);
    }

    #endregion

    #region Absence Management

    /// <summary>
    /// Retrieves all absences recorded by a teacher, with optional semester filtering.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>A list of absences recorded by the teacher</returns>
    /// <response code="200">Returns the list of absences</response>
    /// <response code="404">If no absences are found for this teacher</response>
    [HttpGet("{teacherId:guid}/absences")]
    [ProducesResponseType(typeof(IEnumerable<AbsenceResponseDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<AbsenceResponseDto>>> GetAbsencesByTeacherId(
        Guid teacherId, 
        [FromQuery] Guid? semesterId = null)
    {
        var absences = await _absenceService.GetAbsencesByTeacherAsync(teacherId, semesterId);
        if (!absences.Any())
            return NotFound("No absences found for this teacher");
        return Ok(absences);
    }

    /// <summary>
    /// Creates a new absence record for a student.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher creating the absence</param>
    /// <param name="dto">The absence creation data</param>
    /// <returns>The created absence record</returns>
    /// <response code="201">Returns the newly created absence</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="403">If the teacher is not authorized to record absences for this subject</response>
    /// <response code="404">If referenced entities are not found</response>
    [HttpPost("{teacherId:guid}/absences")]
    [ProducesResponseType(typeof(AbsenceResponseDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AbsenceResponseDto>> CreateAbsence(
        Guid teacherId, CreateAbsenceDto dto)
    {
        var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
        if (!subjects.Any(s => s.Id == dto.SubjectId))
            return Forbid("You are not authorized to assign absences for this subject");

        var absence = await _absenceService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAbsenceById), new { teacherId, absenceId = absence.Id }, absence);
    }

    /// <summary>
    /// Retrieves a specific absence record by ID.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="absenceId">The unique identifier of the absence</param>
    /// <returns>The absence information</returns>
    /// <response code="200">Returns the absence information</response>
    /// <response code="403">If the teacher is not authorized to view this absence</response>
    /// <response code="404">If the absence is not found</response>
    [HttpGet("{teacherId:guid}/absences/{absenceId:guid}")]
    [ProducesResponseType(typeof(AbsenceResponseDto), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AbsenceResponseDto>> GetAbsenceById(Guid teacherId, Guid absenceId)
    {
        var absence = await _absenceService.GetByIdAsync(absenceId);

        // Validate that the teacher has access to this absence
        if (absence.SubjectId.HasValue)
        {
            var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
            if (!subjects.Any(s => s.Id == absence.SubjectId.Value)) 
                return Forbid();
        }

        return Ok(absence);
    }

    /// <summary>
    /// Updates an existing absence record.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="absenceId">The unique identifier of the absence to update</param>
    /// <param name="dto">The updated absence information</param>
    /// <returns>The updated absence information</returns>
    /// <response code="200">Returns the updated absence</response>
    /// <response code="400">If the request data is invalid</response>
    /// <response code="403">If the teacher is not authorized to update this absence</response>
    /// <response code="404">If the absence is not found</response>
    [HttpPut("{teacherId:guid}/absences/{absenceId:guid}")]
    [ProducesResponseType(typeof(AbsenceResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<AbsenceResponseDto>> UpdateAbsence(
        Guid teacherId, Guid absenceId, UpdateAbsenceDto dto)
    {
        var absence = await _absenceService.GetByIdAsync(absenceId);

        // Validate that the teacher has access to this absence
        if (absence.SubjectId.HasValue)
        {
            var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
            if (!subjects.Any(s => s.Id == absence.SubjectId.Value)) 
                return Forbid();
        }

        return await _absenceService.UpdateAsync(absenceId, dto);
    }

    /// <summary>
    /// Deletes an absence record from the system.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="absenceId">The unique identifier of the absence to delete</param>
    /// <returns>No content on successful deletion</returns>
    /// <response code="204">Absence successfully deleted</response>
    /// <response code="403">If the teacher is not authorized to delete this absence</response>
    /// <response code="404">If the absence is not found</response>
    [HttpDelete("{teacherId:guid}/absences/{absenceId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> DeleteAbsence(Guid teacherId, Guid absenceId)
    {
        var absence = await _absenceService.GetByIdAsync(absenceId);

        // Validate that the teacher has access to this absence
        if (absence.SubjectId.HasValue)
        {
            var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
            if (!subjects.Any(s => s.Id == absence.SubjectId.Value)) 
                return Forbid();
        }

        await _absenceService.DeleteAsync(absenceId);
        return NoContent();
    }

    /// <summary>
    /// Retrieves absences for a specific student in a specific subject.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <param name="studentId">The unique identifier of the student</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>A list of absences for the student in the subject</returns>
    /// <response code="200">Returns the list of absences</response>
    /// <response code="403">If the teacher is not authorized to view absences for this subject</response>
    /// <response code="404">If entities are not found</response>
    [HttpGet("{teacherId:guid}/subjects/{subjectId:guid}/students/{studentId:guid}/absences")]
    [ProducesResponseType(typeof(IEnumerable<AbsenceResponseDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<AbsenceResponseDto>>> GetStudentAbsencesBySubject(
        Guid teacherId, Guid subjectId, Guid studentId, [FromQuery] Guid? semesterId = null)
    {
        // Validate that the teacher teaches this subject
        var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
        if (!subjects.Any(s => s.Id == subjectId)) 
            return Forbid();

        var absences = await _absenceService.GetByStudentAndSubjectAsync(studentId, subjectId, semesterId);
        return Ok(absences);
    }

    /// <summary>
    /// Retrieves all absences for a specific subject.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="subjectId">The unique identifier of the subject</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current semester)</param>
    /// <returns>A list of absences for the subject</returns>
    /// <response code="200">Returns the list of absences</response>
    /// <response code="403">If the teacher is not authorized to view absences for this subject</response>
    /// <response code="404">If the subject is not found</response>
    [HttpGet("{teacherId:guid}/subjects/{subjectId:guid}/absences")]
    [ProducesResponseType(typeof(IEnumerable<AbsenceResponseDto>), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<IEnumerable<AbsenceResponseDto>>> GetSubjectAbsences(
        Guid teacherId, Guid subjectId, [FromQuery] Guid? semesterId = null)
    {
        // Validate that the teacher teaches this subject
        var subjects = await _subjectService.GetSubjectsByTeacherAsync(teacherId);
        if (!subjects.Any(s => s.Id == subjectId)) 
            return Forbid();

        var absences = await _absenceService.GetBySubjectAsync(subjectId, semesterId);
        return Ok(absences);
    }

    #endregion

    #region Analytics and Reporting

    /// <summary>
    /// Retrieves comprehensive teacher dashboard data with semester context and grading system integration.
    /// </summary>
    /// <param name="teacherId">The unique identifier of the teacher</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current active semester)</param>
    /// <returns>Dashboard data including subjects, students, recent marks/absences, and statistics</returns>
    /// <response code="200">Returns the teacher dashboard data</response>
    /// <response code="404">If the teacher or semester is not found</response>
    [HttpGet("{teacherId:guid}/dashboard")]
    [ProducesResponseType(typeof(TeacherDashboardDto), 200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<TeacherDashboardDto>> GetTeacherDashboard(
        Guid teacherId, 
        [FromQuery] Guid? semesterId = null)
    {
        var dashboard = await _teacherService.GetTeacherDashboardAsync(teacherId, semesterId);
        return Ok(dashboard);
    }

    /// <summary>
    /// Retrieves attendance overview data for dashboard visualization with semester filtering.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher requesting the data</param>
    /// <param name="fromDate">Optional start date for filtering (format: yyyy-MM-dd). Defaults to semester start.</param>
    /// <param name="toDate">Optional end date for filtering (format: yyyy-MM-dd). Defaults to semester end or current date.</param>
    /// <param name="gradeId">Optional grade ID for filtering specific classes</param>
    /// <param name="subjectId">Optional subject ID for filtering specific subjects</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current active semester)</param>
    /// <param name="daysToShow">Optional number of days to show in trend data (default: 14)</param>
    /// <returns>
    /// A response containing attendance overview data including:
    /// - Total absence count and recent absences (last 7 days)
    /// - Absences categorized by status (Present, Absent, Late, etc.)
    /// - Daily absence trend data for chart visualization
    /// - Semester context information
    /// </returns>
    /// <response code="200">Returns the attendance overview data</response>
    /// <response code="404">If the teacher or semester is not found</response>
    /// <response code="500">If an error occurs during processing</response>
    [HttpGet("{teacherId:guid}/attendance-overview")]
    [ProducesResponseType(typeof(AttendanceOverviewDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAttendanceOverview(
        Guid teacherId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? gradeId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] Guid? semesterId = null,
        [FromQuery] int? daysToShow = 14)
    {
        try
        {
            var filterParams = new AttendanceFilterParams
            {
                FromDate = fromDate,
                ToDate = toDate,
                GradeId = gradeId,
                SubjectId = subjectId,
                SemesterId = semesterId,
                DaysToShow = daysToShow
            };

            var result = await _teacherService.GetAttendanceOverviewAsync(teacherId, filterParams);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance overview for teacher {TeacherId}", teacherId);
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves excused vs unexcused absence breakdown data for analysis with semester filtering.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher requesting the data</param>
    /// <param name="fromDate">Optional start date for filtering (format: yyyy-MM-dd). Defaults to semester start.</param>
    /// <param name="toDate">Optional end date for filtering (format: yyyy-MM-dd). Defaults to semester end or current date.</param>
    /// <param name="gradeId">Optional grade ID for filtering specific classes</param>
    /// <param name="subjectId">Optional subject ID for filtering specific subjects</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current active semester)</param>
    /// <param name="timeFrame">Optional time frame for analysis (week, month, term, year, all)</param>
    /// <returns>
    /// A response containing excused vs unexcused absence data including:
    /// - Total excused and unexcused absence counts and percentages
    /// - Breakdown by subject with counts and percentages
    /// - Semester context information
    /// </returns>
    /// <response code="200">Returns the excused vs unexcused breakdown data</response>
    /// <response code="404">If the teacher or semester is not found</response>
    /// <response code="500">If an error occurs during processing</response>
    [HttpGet("{teacherId:guid}/excused-unexcused-breakdown")]
    [ProducesResponseType(typeof(ExcusedUnexcusedBreakdownDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetExcusedUnexcusedBreakdown(
        Guid teacherId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? gradeId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] Guid? semesterId = null,
        [FromQuery] string? timeFrame = "all")
    {
        try
        {
            var filterParams = new AbsenceBreakdownFilterParams
            {
                FromDate = fromDate,
                ToDate = toDate,
                GradeId = gradeId,
                SubjectId = subjectId,
                SemesterId = semesterId,
                TimeFrame = timeFrame
            };

            var result = await _teacherService.GetExcusedUnexcusedBreakdownAsync(teacherId, filterParams);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving absence breakdown for teacher {TeacherId}", teacherId);
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a list of at-risk students based on semester-specific absence patterns.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher requesting the data</param>
    /// <param name="gradeId">Optional grade ID for filtering specific classes</param>
    /// <param name="subjectId">Optional subject ID for filtering specific subjects</param>
    /// <param name="fromDate">Optional start date for filtering (format: yyyy-MM-dd). Defaults to semester start.</param>
    /// <param name="toDate">Optional end date for filtering (format: yyyy-MM-dd). Defaults to semester end or current date.</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current active semester)</param>
    /// <param name="highRiskThreshold">Optional threshold for high risk classification (default: 20%)</param>
    /// <param name="mediumRiskThreshold">Optional threshold for medium risk classification (default: 10%)</param>
    /// <param name="totalClassDays">Optional total number of class days (auto-calculated from semester if not provided)</param>
    /// <returns>
    /// A response containing at-risk student data including:
    /// - Threshold configuration and total class days used for calculation
    /// - List of at-risk students with absence statistics, risk levels, and recent patterns
    /// - Semester context information
    /// </returns>
    /// <response code="200">Returns the at-risk students data</response>
    /// <response code="404">If the teacher or semester is not found</response>
    /// <response code="500">If an error occurs during processing</response>
    [HttpGet("{teacherId:guid}/at-risk-students")]
    [ProducesResponseType(typeof(AtRiskStudentsResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAtRiskStudents(
        Guid teacherId,
        [FromQuery] Guid? gradeId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? semesterId = null,
        [FromQuery] int? highRiskThreshold = 20,
        [FromQuery] int? mediumRiskThreshold = 10,
        [FromQuery] int? totalClassDays = null)
    {
        try
        {
            var filterParams = new AtRiskStudentsFilterParams
            {
                GradeId = gradeId,
                SubjectId = subjectId,
                FromDate = fromDate,
                ToDate = toDate,
                SemesterId = semesterId,
                HighRiskThreshold = highRiskThreshold,
                MediumRiskThreshold = mediumRiskThreshold,
                TotalClassDays = totalClassDays
            };

            var result = await _teacherService.GetAtRiskStudentsAsync(teacherId, filterParams);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving at-risk students for teacher {TeacherId}", teacherId);
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves comprehensive attendance statistics with semester filtering for data visualization and analytics.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher requesting the data</param>
    /// <param name="fromDate">Optional start date for filtering (format: yyyy-MM-dd). Defaults to semester start.</param>
    /// <param name="toDate">Optional end date for filtering (format: yyyy-MM-dd). Defaults to semester end or current date.</param>
    /// <param name="gradeId">Optional grade ID for filtering specific classes</param>
    /// <param name="subjectId">Optional subject ID for filtering specific subjects</param>
    /// <param name="semesterId">Optional semester ID for filtering (defaults to current active semester)</param>
    /// <returns>
    /// A response containing comprehensive attendance statistics including:
    /// - Overall metrics (total students, total absences, attendance rate)
    /// - Categorized data (absences by status, subject, grade)
    /// - Time-based trends (daily, weekly, monthly)
    /// - Semester context information
    /// </returns>
    /// <response code="200">Returns the attendance statistics data</response>
    /// <response code="404">If the teacher or semester is not found</response>
    /// <response code="500">If an error occurs during processing</response>
    [HttpGet("{teacherId:guid}/attendance-statistics")]
    [ProducesResponseType(typeof(AttendanceStatisticsDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAttendanceStatistics(
        Guid teacherId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] Guid? gradeId = null,
        [FromQuery] Guid? subjectId = null,
        [FromQuery] Guid? semesterId = null)
    {
        try
        {
            var filterParams = new StatisticsFilterParams
            {
                FromDate = fromDate,
                ToDate = toDate,
                GradeId = gradeId,
                SubjectId = subjectId,
                SemesterId = semesterId
            };

            var result = await _teacherService.GetAttendanceStatisticsAsync(teacherId, filterParams);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance statistics for teacher {TeacherId}", teacherId);
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

    #endregion

    #region Search

    /// <summary>
    /// Searches for teachers based on specified criteria with pagination support.
    /// </summary>
    /// <param name="query">General search query (searches in name, email, title)</param>
    /// <param name="institutionId">Filter by institution ID</param>
    /// <param name="departmentId">Filter by department ID</param>
    /// <param name="classGradeId">Filter by class grade ID (homeroom teacher)</param>
    /// <param name="pageNumber">Page number for pagination (1-based, default: 1)</param>
    /// <param name="pageSize">Page size for pagination (1-100, default: 10)</param>
    /// <param name="sortBy">Field to sort by (firstName, lastName, email, title, institution, createdAt)</param>
    /// <param name="ascending">Sort direction (true for ascending, false for descending, default: true)</param>
    /// <returns>
    /// A paginated response containing:
    /// - List of teachers matching the search criteria
    /// - Pagination metadata (total count, page info, etc.)
    /// </returns>
    /// <response code="200">Returns the search results with pagination metadata</response>
    /// <response code="400">If the search parameters are invalid</response>
    /// <response code="500">If an error occurs during the search</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(TeacherSearchResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TeacherSearchResponse>> SearchTeachers(
        [FromQuery] string? query = null,
        [FromQuery] Guid? institutionId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? classGradeId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool ascending = true)
    {
        try
        {
            var searchParams = new TeacherSearchParams
            {
                Query = query,
                InstitutionId = institutionId,
                DepartmentId = departmentId,
                ClassGradeId = classGradeId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                Ascending = ascending
            };

            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var result = await _teacherService.SearchTeachersAsync(searchParams);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching teachers with parameters: {@SearchParams}", new { query, institutionId, departmentId, classGradeId, pageNumber, pageSize, sortBy, ascending });
            return StatusCode(500, $"An error occurred while searching teachers: {ex.Message}");
        }
    }

    #endregion

    #region Grade Assignment Management

    /// <summary>
    /// Assigns a teacher to teach multiple grades directly.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher to assign</param>
    /// <param name="dto">The grade assignment request containing grade IDs</param>
    /// <returns>
    /// A response containing:
    /// - Success status and message
    /// - Teacher ID and assigned grade IDs
    /// - Total number of grades assigned and students impacted
    /// </returns>
    /// <response code="200">Returns the assignment result with success details</response>
    /// <response code="400">If the request data is invalid or validation fails</response>
    /// <response code="404">If the teacher or any specified grades are not found</response>
    /// <response code="500">If an error occurs during the assignment process</response>
    [HttpPost("{teacherId:guid}/grade-assignments")]
    [ProducesResponseType(typeof(TeacherGradeAssignmentResultDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TeacherGradeAssignmentResultDto>> AssignTeacherToGrades(
        Guid teacherId,
        AssignTeacherToGradesDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _teacherService.AssignTeacherToGradesAsync(teacherId, dto);
            return Ok(result);
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
            _logger.LogError(ex, "Error assigning teacher {TeacherId} to grades", teacherId);
            return StatusCode(500, $"An error occurred while assigning grades: {ex.Message}");
        }
    }

    /// <summary>
    /// Removes a teacher's assignment from specific grades.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher to unassign</param>
    /// <param name="dto">The grade unassignment request containing grade IDs</param>
    /// <returns>
    /// A response containing:
    /// - Success status and message
    /// - Teacher ID and unassigned grade IDs
    /// - Total number of grades unassigned and students impacted
    /// </returns>
    /// <response code="200">Returns the unassignment result with success details</response>
    /// <response code="400">If the request data is invalid or teacher is not assigned to specified grades</response>
    /// <response code="404">If the teacher is not found</response>
    /// <response code="500">If an error occurs during the unassignment process</response>
    [HttpDelete("{teacherId:guid}/grade-assignments")]
    [ProducesResponseType(typeof(TeacherGradeAssignmentResultDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TeacherGradeAssignmentResultDto>> UnassignTeacherFromGrades(
        Guid teacherId,
        UnassignTeacherFromGradesDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _teacherService.UnassignTeacherFromGradesAsync(teacherId, dto);
            return Ok(result);
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
            _logger.LogError(ex, "Error unassigning teacher {TeacherId} from grades", teacherId);
            return StatusCode(500, $"An error occurred while unassigning grades: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates all grade assignments for a teacher in one operation.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher whose assignments to update</param>
    /// <param name="dto">The grade assignment update request containing new grade IDs</param>
    /// <returns>
    /// A response containing:
    /// - Success status and message
    /// - Teacher ID and final assigned grade IDs
    /// - Total number of grades assigned and students impacted
    /// </returns>
    /// <response code="200">Returns the update result with success details</response>
    /// <response code="400">If the request data is invalid or validation fails</response>
    /// <response code="404">If the teacher or any specified grades are not found</response>
    /// <response code="500">If an error occurs during the update process</response>
    [HttpPut("{teacherId:guid}/grade-assignments")]
    [ProducesResponseType(typeof(TeacherGradeAssignmentResultDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TeacherGradeAssignmentResultDto>> UpdateTeacherGradeAssignments(
        Guid teacherId,
        UpdateTeacherGradeAssignmentsDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _teacherService.UpdateTeacherGradeAssignmentsAsync(teacherId, dto);
            return Ok(result);
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
            _logger.LogError(ex, "Error updating teacher {TeacherId} grade assignments", teacherId);
            return StatusCode(500, $"An error occurred while updating grade assignments: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a teacher with all their grade assignments and related information.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher to retrieve</param>
    /// <returns>
    /// A detailed response containing:
    /// - Basic teacher information (name, email, title, etc.)
    /// - All assigned grades with details (including homeroom status)
    /// - Assigned subjects information
    /// - Assignment metadata (creation/update dates)
    /// </returns>
    /// <response code="200">Returns the teacher with complete grade assignment details</response>
    /// <response code="404">If the teacher is not found</response>
    /// <response code="500">If an error occurs during retrieval</response>
    [HttpGet("{teacherId:guid}/grade-assignments/detailed")]
    [ProducesResponseType(typeof(TeacherWithGradeAssignmentsResponseDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TeacherWithGradeAssignmentsResponseDto>> GetTeacherWithGradeAssignments(
        Guid teacherId)
    {
        try
        {
            var result = await _teacherService.GetTeacherWithGradeAssignmentsAsync(teacherId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving teacher {TeacherId} with grade assignments", teacherId);
            return StatusCode(500, $"An error occurred while retrieving teacher grade assignments: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves all grades assigned to a specific teacher.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher whose assigned grades to retrieve</param>
    /// <returns>
    /// A list of grade assignments containing:
    /// - Grade information (ID, name, institution, academic year)
    /// - Student count for each grade
    /// - Homeroom indicator
    /// - Assignment metadata
    /// </returns>
    /// <response code="200">Returns the list of assigned grades</response>
    /// <response code="404">If the teacher is not found</response>
    /// <response code="500">If an error occurs during retrieval</response>
    [HttpGet("{teacherId:guid}/grade-assignments")]
    [ProducesResponseType(typeof(IEnumerable<GradeAssignmentResponseDto>), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<IEnumerable<GradeAssignmentResponseDto>>> GetTeacherAssignedGrades(
        Guid teacherId)
    {
        try
        {
            var result = await _teacherService.GetTeacherAssignedGradesAsync(teacherId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving assigned grades for teacher {TeacherId}", teacherId);
            return StatusCode(500, $"An error occurred while retrieving assigned grades: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves a comprehensive summary of a teacher's grade assignments.
    /// </summary>
    /// <param name="teacherId">The ID of the teacher whose assignment summary to retrieve</param>
    /// <returns>
    /// A summary containing:
    /// - Teacher basic information
    /// - Total counts (grades assigned, students impacted)
    /// - Homeroom assignment details
    /// - Detailed breakdown of all grade assignments with types
    /// </returns>
    /// <response code="200">Returns the teacher grade assignment summary</response>
    /// <response code="404">If the teacher is not found</response>
    /// <response code="500">If an error occurs during retrieval</response>
    [HttpGet("{teacherId:guid}/grade-assignments/summary")]
    [ProducesResponseType(typeof(TeacherGradeAssignmentSummaryDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<TeacherGradeAssignmentSummaryDto>> GetTeacherGradeAssignmentSummary(
        Guid teacherId)
    {
        try
        {
            var result = await _teacherService.GetTeacherGradeAssignmentSummaryAsync(teacherId);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving grade assignment summary for teacher {TeacherId}", teacherId);
            return StatusCode(500, $"An error occurred while retrieving assignment summary: {ex.Message}");
        }
    }

    #endregion
}