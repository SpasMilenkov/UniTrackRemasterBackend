using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Department;


public record UpdateDepartmentDto(
    string? Name,
    string? Code,
    string? Description,
    string? Location,
    [EmailAddress] string? ContactEmail,
    [Phone] string? ContactPhone,
    DepartmentType? Type,
    DepartmentStatus? Status);
