using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Department;

public record CreateDepartmentDto(
    [Required] string Name,
    [Required] string Code,
    string? Description,
    string? Location,
    [EmailAddress] string? ContactEmail,
    [Phone] string? ContactPhone,
    [Required] DepartmentType Type,
    [Required] DepartmentStatus Status,
    [Required] Guid FacultyId);