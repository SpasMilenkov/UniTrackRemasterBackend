using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Parent;

/// <summary>
/// DTO for adding a child to a parent
/// </summary>
public class AddChildDto
{
    [Required]
    public required Guid StudentId { get; set; }
}