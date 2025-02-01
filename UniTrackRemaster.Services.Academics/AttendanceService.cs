using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;


public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        ISubjectRepository subjectRepository,
        ILogger<AttendanceService> logger)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _subjectRepository = subjectRepository;
        _logger = logger;
    }

    public async Task<AttendanceResponseDto> CreateAsync(CreateAttendanceDto dto)
    {
        if (!dto.Validate())
            throw new ValidationException("Must specify either Course or Subject, not both or neither");

        var student = await _studentRepository.GetByIdAsync(dto.StudentId);
        if (student == null)
            throw new NotFoundException("Student not found");

        if (dto.CourseId.HasValue)
        {
            var course = await _courseRepository.GetByIdAsync(dto.CourseId.Value);
            if (course == null)
                throw new NotFoundException("Course not found");
        }

        if (dto.SubjectId.HasValue)
        {
            var subject = await _subjectRepository.GetByIdAsync(dto.SubjectId.Value);
            if (subject == null)
                throw new NotFoundException("Subject not found");
        }

        var attendance = new Attendance
        {
            Id = Guid.NewGuid(),
            Date = dto.Date,
            Status = dto.Status,
            Reason = dto.Reason,
            IsExcused = dto.IsExcused,
            StudentId = dto.StudentId,
            CourseId = dto.CourseId,
            SubjectId = dto.SubjectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _attendanceRepository.CreateAsync(attendance);
        return AttendanceResponseDto.FromEntity(attendance);
    }

    public async Task<AttendanceResponseDto> UpdateAsync(Guid id, UpdateAttendanceDto dto)
    {
        var attendance = await _attendanceRepository.GetByIdAsync(id);
        if (attendance == null)
            throw new NotFoundException("Attendance record not found");

        if (dto.Status.HasValue)
            attendance.Status = dto.Status.Value;
        if (dto.Reason != null)
            attendance.Reason = dto.Reason;
        if (dto.IsExcused.HasValue)
            attendance.IsExcused = dto.IsExcused.Value;

        attendance.UpdatedAt = DateTime.UtcNow;
        await _attendanceRepository.UpdateAsync(attendance);
        return AttendanceResponseDto.FromEntity(attendance);
    }

    public async Task<AttendanceResponseDto> GetByIdAsync(Guid id)
    {
        var attendance = await _attendanceRepository.GetByIdAsync(id);
        if (attendance == null)
            throw new NotFoundException("Attendance record not found");
        return AttendanceResponseDto.FromEntity(attendance);
    }

    public async Task<IEnumerable<AttendanceResponseDto>> GetByStudentAsync(Guid studentId)
    {
        var attendances = await _attendanceRepository.GetByStudentAsync(studentId);
        return attendances.Select(AttendanceResponseDto.FromEntity);
    }

    public async Task<IEnumerable<AttendanceResponseDto>> GetByCourseAsync(Guid courseId)
    {
        var attendances = await _attendanceRepository.GetByCourseAsync(courseId);
        return attendances.Select(AttendanceResponseDto.FromEntity);
    }

    public async Task<IEnumerable<AttendanceResponseDto>> GetBySubjectAsync(Guid subjectId)
    {
        var attendances = await _attendanceRepository.GetBySubjectAsync(subjectId);
        return attendances.Select(AttendanceResponseDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var attendance = await _attendanceRepository.GetByIdAsync(id);
        if (attendance == null)
            throw new NotFoundException("Attendance record not found");

        await _attendanceRepository.DeleteAsync(id);
    }
}