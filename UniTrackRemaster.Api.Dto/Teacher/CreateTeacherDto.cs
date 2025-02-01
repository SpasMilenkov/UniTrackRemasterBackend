using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateTeacherDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] [EmailAddress] string Email,
    [Required] string Password,
    [Required] string Title,
    [Required] Guid InstitutionId,
    Guid? ClassGradeId = null)
{
    public static Teacher ToEntity(CreateTeacherDto dto, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Title = dto.Title,
        InstitutionId = dto.InstitutionId,
        ClassGradeId = dto.ClassGradeId
    };
}