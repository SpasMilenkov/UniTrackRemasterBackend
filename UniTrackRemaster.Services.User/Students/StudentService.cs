using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.User.Students;

public class StudentService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    ILogger<StudentService> logger)
    : IStudentService
{
    public async Task<StudentResponseDto> CreateAsync(CreateStudentDto dto)
    {
        // Validate student type requirements
        if (dto.IsSchoolStudent && !dto.SchoolId.HasValue)
            throw new ValidationException("School ID is required for school students");
        if (dto.IsUniversityStudent && !dto.UniversityId.HasValue)
            throw new ValidationException("University ID is required for university students");

        await unitOfWork.BeginTransactionAsync();
        try
        {
            // Check if user exists
            var user = await userManager.FindByEmailAsync(dto.Email) ??
                throw new NotFoundException($"Application user with email {dto.Email} not found. Please create a user account first.");

            // Check if user already has a student profile (any status)
            var existingStudent = await unitOfWork.Students.GetByUserIdAsync(user.Id);
            if (existingStudent != null)
                throw new InvalidOperationException($"User with email {dto.Email} already has a student profile.");

            // Validate institutions exist
            if (dto.SchoolId.HasValue)
            {
                var school = await unitOfWork.Schools.GetByIdAsync(dto.SchoolId.Value);
                if (school == null)
                    throw new NotFoundException("School not found");
            }

            if (dto.UniversityId.HasValue)
            {
                var university = await unitOfWork.Universities.GetByIdAsync(dto.UniversityId.Value);
                if (university == null)
                    throw new NotFoundException("University not found");
            }

            // Validate grade exists
            var grade = await unitOfWork.Grades.GetByIdAsync(dto.GradeId);
            if (grade == null)
                throw new NotFoundException("Grade not found");

            // Create student entity with PENDING status
            // DO NOT link institutions or add roles yet
            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                IsSchoolStudent = dto.IsSchoolStudent,
                SchoolId = dto.SchoolId,
                UniversityId = dto.UniversityId,
                GradeId = dto.GradeId,
                Status = Data.Models.Enums.ProfileStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await unitOfWork.Students.CreateAsync(student);
            await unitOfWork.CommitAsync();

            return StudentResponseDto.FromEntity(student);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error creating student invitation: {Message}", ex.Message);
            throw;
        }
    }
    public async Task<StudentResponseDto> UpdateAsync(Guid id, UpdateStudentDto dto)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            var student = await unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
                throw new NotFoundException("Student not found");

            if (dto.IsSchoolStudent.HasValue)
                student.IsSchoolStudent = dto.IsSchoolStudent.Value;
            

            if (dto.SchoolId.HasValue)
            {
                var school = await unitOfWork.Schools.GetByIdAsync(dto.SchoolId.Value);
                if (school == null)
                    throw new NotFoundException("School not found");
                student.SchoolId = dto.SchoolId;
            }

            if (dto.UniversityId.HasValue)
            {
                var university = await unitOfWork.Universities.GetByIdAsync(dto.UniversityId.Value);
                if (university == null)
                    throw new NotFoundException("University not found");
                student.UniversityId = dto.UniversityId.Value;
            }

            if (dto.GradeId.HasValue)
            {
                var grade = await unitOfWork.Grades.GetByIdAsync(dto.GradeId.Value);
                if (grade == null)
                    throw new NotFoundException("Grade not found");
                student.GradeId = dto.GradeId.Value;
            }

            student.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.Students.UpdateAsync(student);
            await unitOfWork.CommitAsync();

            return StudentResponseDto.FromEntity(student);
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
    
    public async Task<PaginatedStudentResponseDto> SearchWithPaginationAsync(StudentSearchRequestDto request)
    {
        try
        {
            // Validate pagination parameters
            if (request.PageNumber < 1) request.PageNumber = 1;
            if (request.PageSize < 1) request.PageSize = 20;
            if (request.PageSize > 100) request.PageSize = 100; // Max page size limit

            var (students, totalCount) = await unitOfWork.Students.SearchWithPaginationAsync(
                request.Query,
                request.GradeId,
                request.InstitutionId,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.Ascending);

            var studentDtos = students.Select(StudentResponseDto.FromEntity);

            return PaginatedStudentResponseDto.Create(
                studentDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching students with pagination. Query: {Query}, Page: {Page}, PageSize: {PageSize}", 
                request.Query, request.PageNumber, request.PageSize);
            throw;
        }
    }

    public async Task<StudentResponseDto> GetByIdAsync(Guid id)
    {
        var student = await unitOfWork.Students.GetByIdAsync(id);
        if (student == null)
            throw new NotFoundException("Student not found");
        return StudentResponseDto.FromEntity(student);
    }
    public async Task<IEnumerable<StudentResponseDto>> GetBySchoolAsync(Guid schoolId)
    {
        var students = await unitOfWork.Students.GetBySchoolAsync(schoolId);
        return students.Select(StudentResponseDto.FromEntity);
    }

    public async Task<IEnumerable<StudentResponseDto>> GetByUniversityAsync(Guid universityId)
    {
        var students = await unitOfWork.Students.GetByUniversityAsync(universityId);
        return students.Select(StudentResponseDto.FromEntity);
    }

    public async Task<IEnumerable<StudentResponseDto>> GetByGradeAsync(Guid gradeId)
    {
        var students = await unitOfWork.Students.GetByGradeIdAsync(gradeId);
        return students.Select(StudentResponseDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await unitOfWork.BeginTransactionAsync();

        try
        {
            var student = await unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
                throw new NotFoundException("Student not found");

            if (student.Marks?.Any() == true ||
                student.AttendanceRecords?.Any() == true ||
                student.Electives?.Any() == true)
                throw new ValidationException("Cannot delete student with associated records");

            await userManager.DeleteAsync(student.User);
            await unitOfWork.Students.DeleteAsync(id);
            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<StudentResponseDto> GetByUserIdAsync(Guid userId)
    {
        var student = await unitOfWork.Students.GetByUserIdAsync(userId) ?? throw new NotFoundException("Student with that ID does not exist");
        return StudentResponseDto.FromEntity(student);
    }
}