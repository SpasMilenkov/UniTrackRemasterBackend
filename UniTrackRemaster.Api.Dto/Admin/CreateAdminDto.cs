using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Request;

public record CreateAdminDto(
    [Required] string FirstName,
    [Required] string LastName,
    [Required] [EmailAddress] string Email,
    [Required] string Password,
    [Required] string Position,
    string? Department,
    [Required] AdminRole Role,
    [Required] Guid InstitutionId,
    IList<PermissionType> Permissions = null)
{
    public static Admin ToEntity(CreateAdminDto dto, Guid userId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Position = dto.Position,
        Department = dto.Department,
        StartDate = DateTime.UtcNow,
        Role = dto.Role,
        Status = AdminStatus.Active,
        InstitutionId = dto.InstitutionId,
        Permissions = dto.Permissions?.Select(p => new AdminPermission
        {
            Permission = p,
            AdminId = userId
        }).ToList() ?? new List<AdminPermission>()
    };
}
