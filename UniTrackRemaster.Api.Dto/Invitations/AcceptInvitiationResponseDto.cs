namespace UniTrackRemaster.Api.Dto.Invitations;

public record AcceptInvitationResponseDto
{
    public string ProfileType { get; init; } = string.Empty;
    public string InstitutionName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime AcceptedAt { get; init; }
}