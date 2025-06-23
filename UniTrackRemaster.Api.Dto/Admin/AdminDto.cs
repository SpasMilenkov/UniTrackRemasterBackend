using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Admin;

public record AdminDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Position,
    string? Department,
    DateTime StartDate,
    AdminRole Role,
    ProfileStatus Status,
    string? Notes,
    Guid InstitutionId,
    string InstitutionName,
    ProfileStatus ProfileStatus)
{
    public static AdminDto FromEntity(Data.Models.Users.Admin admin) => new(
        admin.Id,
        admin.User?.FirstName ?? "",
        admin.User?.LastName ?? "",
        admin.User?.Email ?? "",
        admin.Position,
        admin.Department,
        admin.StartDate,
        admin.Role,
        admin.Status,
        admin.Notes,
        admin.InstitutionId,
        admin.Institution?.Name ?? "",
        admin.Status
    );
}