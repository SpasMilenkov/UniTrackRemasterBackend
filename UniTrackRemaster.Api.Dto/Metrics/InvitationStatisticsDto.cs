using UniTrackRemaster.Api.Dto.Invitations;

namespace UniTrackRemaster.Api.Dto.Metrics;

public record InvitationStatisticsDto
{
    public int TotalInvitations { get; init; }
    public int PendingCount { get; init; }
    public int AcceptedCount { get; init; }
    public int RejectedCount { get; init; }
    public int InactiveCount { get; init; }
    public double AcceptanceRate { get; init; }
    public Dictionary<string, int> InvitationsByType { get; init; } = [];
    public IEnumerable<InstitutionInvitationDto> RecentInvitations { get; init; } = new List<InstitutionInvitationDto>();
}