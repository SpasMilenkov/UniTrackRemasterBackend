using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Faculty;

public record CreateFacultyDto(
    [Required] string Name,
    [Required] string Code,
    [Required] string ShortDescription,
    [Required] string DetailedDescription,
    string? Website,
    [EmailAddress] string? ContactEmail,
    [Phone] string? ContactPhone,
    [Required] FacultyStatus Status,
    [Required] Guid UniversityId);