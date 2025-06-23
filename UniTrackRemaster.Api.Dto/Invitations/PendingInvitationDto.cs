using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Invitations;

public record PendingInvitationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty; // "Student", "Teacher", "Admin"
    public Guid InstitutionId { get; init; }
    public string InstitutionName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty; // Title for teachers, role for admins, etc.
    public string? GradeName { get; init; }
    public string? AdditionalInfo { get; init; }
    public DateTime InvitedAt { get; init; }
    public ProfileStatus Status { get; init; }
}
