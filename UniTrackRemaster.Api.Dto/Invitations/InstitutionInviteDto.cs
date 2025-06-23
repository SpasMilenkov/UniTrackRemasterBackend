using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Invitations;

public record InstitutionInvitationDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty; // "Student", "Teacher", "Admin"
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public ProfileStatus Status { get; init; }
    public DateTime InvitedAt { get; init; }
    public DateTime? AcceptedAt { get; init; }
}