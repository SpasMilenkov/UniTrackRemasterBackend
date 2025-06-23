using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Admin;

public record UpdateAdminDto(
    string? Position = null,
    string? Department = null,
    AdminRole? Role = null,
    ProfileStatus? Status = null,
    string? Notes = null)
{
    public void UpdateEntity(Data.Models.Users.Admin admin)
    {
        if (Position != null) admin.Position = Position;
        if (Department != null) admin.Department = Department;
        if (Role.HasValue) admin.Role = Role.Value;
        if (Status.HasValue) admin.Status = Status.Value;
        if (Notes != null) admin.Notes = Notes;
    }
}