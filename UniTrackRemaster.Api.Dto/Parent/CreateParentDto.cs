using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Parent;

/// <summary>
/// DTO for creating a new parent profile
/// </summary>
public class CreateParentDto
{
    [Required]
    public required Guid UserId { get; set; }

    [StringLength(100)]
    public string? Occupation { get; set; }

    [StringLength(20)]
    [Phone]
    public string? EmergencyContact { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public static Data.Models.Users.Parent ToEntity(CreateParentDto dto)
    {
        return new Data.Models.Users.Parent
        {
            Id = Guid.NewGuid(),
            UserId = dto.UserId,
            Occupation = dto.Occupation,
            EmergencyContact = dto.EmergencyContact,
            Notes = dto.Notes,
            Status = ProfileStatus.Pending,
            Children = new List<Data.Models.Users.Student>()
        };
    }
}
