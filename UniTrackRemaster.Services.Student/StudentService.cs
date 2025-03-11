using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Student;

public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IInstitutionRepository _institutionRepository;
    private readonly IGradeRepository _gradeRepository;
    private readonly ILogger<StudentService> _logger;

    public StudentService(
        IStudentRepository studentRepository,
        UserManager<ApplicationUser> userManager,
        IInstitutionRepository institutionRepository,
        IGradeRepository gradeRepository,
        ILogger<StudentService> logger)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _institutionRepository = institutionRepository;
        _gradeRepository = gradeRepository;
        _logger = logger;
    }

    public async Task<StudentResponseDto> CreateAsync(CreateStudentDto dto)
    {
        if (dto.IsSchoolStudent && !dto.SchoolId.HasValue)
            throw new ValidationException("School ID is required for school students");

        if (dto.IsUniversityStudent && !dto.UniversityId.HasValue)
            throw new ValidationException("University ID is required for university students");

        var grade = await _gradeRepository.GetByIdAsync(dto.GradeId);
        if (grade == null)
            throw new NotFoundException("Grade not found");

        if (dto.SchoolId.HasValue)
        {
            var school = await _institutionRepository.GetByIdAsync(dto.SchoolId.Value);
            if (school == null)
                throw new NotFoundException("School not found");
        }

        if (dto.UniversityId.HasValue)
        {
            var university = await _institutionRepository.GetByIdAsync(dto.UniversityId.Value);
            if (university == null)
                throw new NotFoundException("University not found");
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            AvatarUrl = null
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new ValidationException(result.Errors.First().Description);

        await _userManager.AddToRoleAsync(user, "Student");

        var student = new UniTrackRemaster.Data.Models.Users.Student
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsSchoolStudent = dto.IsSchoolStudent,
            IsUniversityStudent = dto.IsUniversityStudent,
            SchoolId = dto.SchoolId,
            UniversityId = dto.UniversityId ?? Guid.Empty,
            GradeId = dto.GradeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _studentRepository.CreateAsync(student);
        return StudentResponseDto.FromEntity(student);
    }

    public async Task<StudentResponseDto> UpdateAsync(Guid id, UpdateStudentDto dto)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
            throw new NotFoundException("Student not found");

        if (dto.IsSchoolStudent.HasValue)
            student.IsSchoolStudent = dto.IsSchoolStudent.Value;

        if (dto.IsUniversityStudent.HasValue)
            student.IsUniversityStudent = dto.IsUniversityStudent.Value;

        if (dto.SchoolId.HasValue)
        {
            var school = await _institutionRepository.GetByIdAsync(dto.SchoolId.Value);
            if (school == null)
                throw new NotFoundException("School not found");
            student.SchoolId = dto.SchoolId;
        }

        if (dto.UniversityId.HasValue)
        {
            var university = await _institutionRepository.GetByIdAsync(dto.UniversityId.Value);
            if (university == null)
                throw new NotFoundException("University not found");
            student.UniversityId = dto.UniversityId.Value;
        }

        if (dto.GradeId.HasValue)
        {
            var grade = await _gradeRepository.GetByIdAsync(dto.GradeId.Value);
            if (grade == null)
                throw new NotFoundException("Grade not found");
            student.GradeId = dto.GradeId.Value;
        }

        student.UpdatedAt = DateTime.UtcNow;
        await _studentRepository.UpdateAsync(student);
        return StudentResponseDto.FromEntity(student);
    }

    public async Task<StudentResponseDto> GetByIdAsync(Guid id)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
            throw new NotFoundException("Student not found");
        return StudentResponseDto.FromEntity(student);
    }

    public async Task<IEnumerable<StudentResponseDto>> GetBySchoolAsync(Guid schoolId)
    {
        var students = await _studentRepository.GetBySchoolAsync(schoolId);
        return students.Select(StudentResponseDto.FromEntity);
    }

    public async Task<IEnumerable<StudentResponseDto>> GetByUniversityAsync(Guid universityId)
    {
        var students = await _studentRepository.GetByUniversityAsync(universityId);
        return students.Select(StudentResponseDto.FromEntity);
    }

    public async Task<IEnumerable<StudentResponseDto>> GetByGradeAsync(Guid gradeId)
    {
        var students = await _studentRepository.GetByGradeAsync(gradeId);
        return students.Select(StudentResponseDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null)
            throw new NotFoundException("Student not found");

        if (student.Marks?.Any() == true ||
            student.AttendanceRecords?.Any() == true ||
            student.ClubMemberships?.Any() == true ||
            student.Electives?.Any() == true)
        {
            throw new ValidationException("Cannot delete student with associated records");
        }

        await _userManager.DeleteAsync(student.User);
        await _studentRepository.DeleteAsync(id);
    }
}
