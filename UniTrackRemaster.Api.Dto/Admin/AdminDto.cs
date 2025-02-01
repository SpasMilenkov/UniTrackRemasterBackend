using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Api.Dto.Response;

public record AdminDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Position,
    string? Department,
    DateTime StartDate,
    AdminRole Role,
    AdminStatus Status,
    string? Notes,
    Guid InstitutionId,
    string InstitutionName)
{
    public static AdminDto FromEntity(Admin admin) => new(
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
        admin.Institution?.Name ?? ""
    );
}