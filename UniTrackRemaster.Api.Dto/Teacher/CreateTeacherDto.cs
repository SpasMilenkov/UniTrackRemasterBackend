using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Teacher;

public record CreateTeacherDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] [EmailAddress] string Email,
    [Required] string Title,
    [Required] Guid InstitutionId,
    Guid? ClassGradeId = null)
{
    public static Data.Models.Users.Teacher ToEntity(CreateTeacherDto dto, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = dto.Title,
        InstitutionId = dto.InstitutionId,
        ClassGradeId = dto.ClassGradeId
    };
}