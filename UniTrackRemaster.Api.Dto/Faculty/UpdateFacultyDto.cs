using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Request;

public record UpdateFacultyDto(
    string? Name,
    string? Code,
    string? ShortDescription,
    string? DetailedDescription,
    string? Website,
    [EmailAddress] string? ContactEmail,
    [Phone] string? ContactPhone,
    FacultyStatus? Status);
