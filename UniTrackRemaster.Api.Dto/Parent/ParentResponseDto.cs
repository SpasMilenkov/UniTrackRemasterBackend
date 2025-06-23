using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Parent;

/// <summary>
/// Response DTO for parent information
/// </summary>
public class ParentResponseDto
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public required ProfileStatus Status { get; set; }
    public string? Notes { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required List<ChildResponseDto> Children { get; set; }

    public static ParentResponseDto FromEntity(Data.Models.Users.Parent parent)
    {
        if (parent.User == null)
        {
            throw new ArgumentException("Parent entity must include User navigation property", nameof(parent));
        }

        return new ParentResponseDto
        {
            Id = parent.Id,
            UserId = parent.UserId,
            FirstName = parent.User.FirstName,
            LastName = parent.User.LastName,
            Email = parent.User.Email!,
            Occupation = parent.Occupation,
            EmergencyContact = parent.EmergencyContact,
            Status = parent.Status,
            Notes = parent.Notes,
            CreatedAt = parent.CreatedAt,
            UpdatedAt = parent.UpdatedAt,
            Children = parent.Children?.Select(ChildResponseDto.FromEntity).ToList() ?? new List<ChildResponseDto>()
        };
    }
}