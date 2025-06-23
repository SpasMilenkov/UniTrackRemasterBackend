using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Invitations;

public record ResendInvitationDto
{
    [Required]
    public Guid ProfileId { get; init; }

    [Required]
    public string ProfileType { get; init; } = string.Empty; // "Student", "Teacher", "Admin"
}