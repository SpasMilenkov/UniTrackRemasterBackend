using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateStudentDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] [EmailAddress] string Email,
    [Required] string Password,
    [Required] bool IsSchoolStudent,
    [Required] bool IsUniversityStudent,
    Guid? SchoolId,
    Guid? UniversityId,
    [Required] Guid GradeId);