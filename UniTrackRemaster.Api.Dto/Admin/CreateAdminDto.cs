using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Api.Dto.Admin;

public record CreateAdminDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required][EmailAddress] string Email,
    [Required] string Position,
    string? Department,
    [Required] AdminRole Role,
    [Required] Guid InstitutionId
)
{
    public static Data.Models.Users.Admin ToEntity(CreateAdminDto dto, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Position = dto.Position,
        Department = dto.Department,
        StartDate = DateTime.UtcNow,
        Role = dto.Role,
        InstitutionId = dto.InstitutionId,
    };
}
