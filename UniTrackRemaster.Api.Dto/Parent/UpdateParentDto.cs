using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Parent;

/// <summary>
/// DTO for updating parent profile information
/// </summary>
public class UpdateParentDto
{
    [StringLength(100)]
    public string? Occupation { get; set; }

    [StringLength(20)]
    [Phone]
    public string? EmergencyContact { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public ProfileStatus? Status { get; set; }

    public static Data.Models.Users.Parent ToEntity(UpdateParentDto dto)
    {
        return new Data.Models.Users.Parent
        {
            Id = Guid.Empty, // Will be set by the update method
            UserId = Guid.Empty, // Will be set by the update method
            Occupation = dto.Occupation,
            EmergencyContact = dto.EmergencyContact,
            Notes = dto.Notes,
            Status = dto.Status ?? ProfileStatus.Pending,
            Children = new List<Data.Models.Users.Student>()
        };
    }
}