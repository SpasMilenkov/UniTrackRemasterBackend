using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateAdminDto(
    string? Position = null,
    string? Department = null,
    AdminRole? Role = null,
    AdminStatus? Status = null,
    string? Notes = null,
    IList<PermissionType>? Permissions = null)
{
    public void UpdateEntity(Admin admin)
    {
        if (Position != null) admin.Position = Position;
        if (Department != null) admin.Department = Department;
        if (Role.HasValue) admin.Role = Role.Value;
        if (Status.HasValue) admin.Status = Status.Value;
        if (Notes != null) admin.Notes = Notes;
        if (Permissions != null)
        {
            admin.Permissions = Permissions.Select(p => new AdminPermission
            {
                Permission = p,
                AdminId = admin.Id
            }).ToList();
        }
    }
}