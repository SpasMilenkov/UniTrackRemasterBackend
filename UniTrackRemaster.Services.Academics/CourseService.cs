using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Course;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly IMajorRepository _majorRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        ISubjectRepository subjectRepository,
        IMajorRepository majorRepository,
        ISemesterRepository semesterRepository,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _subjectRepository = subjectRepository;
        _majorRepository = majorRepository;
        _semesterRepository = semesterRepository;
        _logger = logger;
    }

    public async Task<CourseResponseDto> CreateAsync(CreateCourseDto dto)
    {
        var subject = await _subjectRepository.GetByIdAsync(dto.SubjectId);
        if (subject == null)
            throw new NotFoundException("Subject not found");

        if (dto.MajorId.HasValue)
        {
            var major = await _majorRepository.GetByIdAsync(dto.MajorId.Value);
            if (major == null)
                throw new NotFoundException("Major not found");
        }

        var semester = await _semesterRepository.GetByIdAsync(dto.SemesterId);
        if (semester == null)
            throw new NotFoundException("Semester not found");

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Code = dto.Code,
            Name = dto.Name,
            Description = dto.Description,
            Credits = dto.Credits,
            Type = dto.Type,
            SubjectId = dto.SubjectId,
            MajorId = dto.MajorId,
            SemesterId = dto.SemesterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _courseRepository.CreateAsync(course);
        return CourseResponseDto.FromEntity(course);
    }

    public async Task<CourseResponseDto> UpdateAsync(Guid id, UpdateCourseDto dto)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            throw new NotFoundException("Course not found");

        if (dto.Code != null) course.Code = dto.Code;
        if (dto.Name != null) course.Name = dto.Name;
        if (dto.Description != null) course.Description = dto.Description;
        if (dto.Credits.HasValue) course.Credits = dto.Credits.Value;
        if (dto.Type.HasValue) course.Type = dto.Type.Value;

        course.UpdatedAt = DateTime.UtcNow;
        await _courseRepository.UpdateAsync(course);
        return CourseResponseDto.FromEntity(course);
    }

    public async Task<CourseResponseDto> GetByIdAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            throw new NotFoundException("Course not found");
        return CourseResponseDto.FromEntity(course);
    }

    public async Task<IEnumerable<CourseResponseDto>> GetBySemesterAsync(Guid semesterId)
    {
        var courses = await _courseRepository.GetBySemesterAsync(semesterId);
        return courses.Select(CourseResponseDto.FromEntity);
    }

    public async Task<IEnumerable<CourseResponseDto>> GetBySubjectAsync(Guid subjectId)
    {
        var courses = await _courseRepository.GetBySubjectAsync(subjectId);
        return courses.Select(CourseResponseDto.FromEntity);
    }

    public async Task<IEnumerable<CourseResponseDto>> GetByMajorAsync(Guid majorId)
    {
        var courses = await _courseRepository.GetByMajorAsync(majorId);
        return courses.Select(CourseResponseDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var course = await _courseRepository.GetByIdAsync(id);
        if (course == null)
            throw new NotFoundException("Course not found");

        if (course.StudentCourses?.Any() == true)
            throw new ValidationException("Cannot delete course with enrolled students");

        if (course.Assignments?.Any() == true)
            throw new ValidationException("Cannot delete course with existing assignments");

        await _courseRepository.DeleteAsync(id);
    }
}