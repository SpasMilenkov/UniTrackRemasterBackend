using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Parent;

/// <summary>
/// Response DTO for child information within parent context
/// </summary>
public class ChildResponseDto
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required bool IsSchoolStudent { get; set; }
    public required ProfileStatus Status { get; set; }
    public string? GradeName { get; set; }
    public string? MajorName { get; set; }

    public static ChildResponseDto FromEntity(Data.Models.Users.Student student)
    {
        if (student.User == null)
        {
            throw new ArgumentException("Student entity must include User navigation property", nameof(student));
        }

        return new ChildResponseDto
        {
            Id = student.Id,
            UserId = student.UserId,
            FirstName = student.User.FirstName,
            LastName = student.User.LastName,
            Email = student.User.Email!,
            IsSchoolStudent = student.IsSchoolStudent,
            Status = student.Status,
            GradeName = student.Grade?.Name,
            MajorName = student.Major?.Name
        };
    }
}