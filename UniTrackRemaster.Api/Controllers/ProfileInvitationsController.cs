using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Invitations;
using UniTrackRemaster.Api.Dto.Metrics;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.Organization;

namespace UniTrackRemaster.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileInvitationsController(IProfileInvitationService invitationService) : ControllerBase
    {
        /// <summary>
        /// Get pending invitations for the current user
        /// </summary>
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<PendingInvitationDto>>> GetPendingInvitations()
        {
            try
            {
                var userId = GetCurrentUserId();
                var invitations = await invitationService.GetPendingInvitationsAsync(userId);
                return Ok(invitations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Accept a profile invitation
        /// </summary>
        [HttpPost("accept")]
        public async Task<ActionResult<AcceptInvitationResponseDto>> AcceptInvitation([FromBody] AcceptInvitationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await invitationService.AcceptInvitationAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Decline a profile invitation
        /// </summary>
        [HttpPost("decline")]
        public async Task<ActionResult> DeclineInvitation([FromBody] DeclineInvitationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await invitationService.DeclineInvitationAsync(userId, dto);
                return Ok(new { message = "Invitation declined successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin: Get all invitations for an institution
        /// </summary>
        [HttpGet("institution/{institutionId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<IEnumerable<InstitutionInvitationDto>>> GetInstitutionInvitations(
            Guid institutionId,
            [FromQuery] ProfileStatus? status = null)
        {
            try
            {
                var invitations = await invitationService.GetInstitutionInvitationsAsync(institutionId, status);
                return Ok(invitations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin: Resend a declined invitation
        /// </summary>
        [HttpPost("resend")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> ResendInvitation([FromBody] ResendInvitationDto dto)
        {
            try
            {
                await invitationService.ResendInvitationAsync(dto);
                return Ok(new { message = "Invitation resent successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Admin: Cancel a pending invitation
        /// </summary>
        [HttpDelete("cancel")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult> CancelInvitation([FromBody] CancelInvitationDto dto)
        {
            try
            {
                await invitationService.CancelInvitationAsync(dto.ProfileId, dto.ProfileType);
                return Ok(new { message = "Invitation cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get invitation statistics for an institution
        /// </summary>
        [HttpGet("institution/{institutionId}/statistics")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<InvitationStatisticsDto>> GetInvitationStatistics(Guid institutionId)
        {
            try
            {
                var allInvitations = await invitationService.GetInstitutionInvitationsAsync(institutionId);

                var stats = new InvitationStatisticsDto
                {
                    TotalInvitations = allInvitations.Count(),
                    PendingCount = allInvitations.Count(i => i.Status == ProfileStatus.Pending),
                    AcceptedCount = allInvitations.Count(i => i.Status == ProfileStatus.Active),
                    RejectedCount = allInvitations.Count(i => i.Status == ProfileStatus.Rejected),
                    InactiveCount = allInvitations.Count(i => i.Status == ProfileStatus.Inactive),
                    AcceptanceRate = allInvitations.Any() ?
                        Math.Round((double)allInvitations.Count(i => i.Status == ProfileStatus.Active) / allInvitations.Count() * 100, 1) : 0,
                    InvitationsByType = allInvitations.GroupBy(i => i.Type).ToDictionary(g => g.Key, g => g.Count()),
                    RecentInvitations = allInvitations.OrderByDescending(i => i.InvitedAt).Take(5).ToList()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Check if current user has any pending invitations
        /// </summary>
        [HttpGet("has-pending")]
        public async Task<ActionResult<bool>> HasPendingInvitations()
        {
            try
            {
                var userId = GetCurrentUserId();
                var invitations = await invitationService.GetPendingInvitationsAsync(userId);
                return Ok(invitations.Any());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get pending invitations count for current user
        /// </summary>
        [HttpGet("pending/count")]
        public async Task<ActionResult<int>> GetPendingInvitationsCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var invitations = await invitationService.GetPendingInvitationsAsync(userId);
                return Ok(invitations.Count());
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }
    }

}
