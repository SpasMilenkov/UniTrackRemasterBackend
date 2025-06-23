using UniTrackRemaster.Api.Dto.Invitations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Commons.Services;

public interface IProfileInvitationService
{
    // Get pending invitations for a user
    Task<IEnumerable<PendingInvitationDto>> GetPendingInvitationsAsync(Guid userId);

    // Accept an invitation
    Task<AcceptInvitationResponseDto> AcceptInvitationAsync(Guid userId, AcceptInvitationDto dto);

    // Decline an invitation  
    Task DeclineInvitationAsync(Guid userId, DeclineInvitationDto dto);

    // Admin: Resend invitation (change status from Rejected back to Pending)
    Task ResendInvitationAsync(ResendInvitationDto dto);

    // Admin: Get all invitations for an institution with filtering
    Task<IEnumerable<InstitutionInvitationDto>> GetInstitutionInvitationsAsync(Guid institutionId, ProfileStatus? status = null);

    // Admin: Cancel pending invitation
    Task CancelInvitationAsync(Guid profileId, string profileType);
}