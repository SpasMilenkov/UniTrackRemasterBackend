using UniTrackRemaster.Api.Dto.Student;
using UniTrackRemaster.Api.Dto.Subject;
using UniTrackRemaster.Api.Dto.Teacher;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Mark;

/// <summary>
/// Mark response DTO with grading system information
/// </summary>
public class MarkResponseDto
{
    /// <summary>Mark ID</summary>
    public Guid Id { get; set; }

    /// <summary>Numerical score (0-100 scale)</summary>
    public decimal Value { get; set; }

    /// <summary>Institution-specific grade (A, B, 5, etc.) - converted from score</summary>
    public string? Grade { get; set; }

    /// <summary>GPA points for this mark based on institution's grading system</summary>
    public double? GpaPoints { get; set; }

    /// <summary>Status (Completed/Failed) based on institution's passing score</summary>
    public string? Status { get; set; }

    /// <summary>Topic or assignment name</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>Optional description of the mark</summary>
    public string? Description { get; set; }

    /// <summary>Type of mark (Test, Assignment, Quiz, etc.)</summary>
    public MarkType Type { get; set; }

    /// <summary>Subject information</summary>
    public SubjectResponseDto? Subject { get; set; }

    /// <summary>Teacher information</summary>
    public TeacherResponseDto? Teacher { get; set; }

    /// <summary>Student information</summary>
    public StudentResponseDto? Student { get; set; }

    /// <summary>Semester ID this mark belongs to</summary>
    public Guid? SemesterId { get; set; }

    /// <summary>Date when the mark was created</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Date when the mark was last updated</summary>
    public DateTime UpdatedAt { get; set; }

    public static MarkResponseDto FromEntity(Data.Models.Academical.Mark mark)
    {
        return new MarkResponseDto
        {
            Id = mark.Id,
            Value = mark.Value,
            Topic = mark.Topic,
            Description = mark.Description,
            Type = mark.Type,
            Subject = mark.Subject != null ? SubjectResponseDto.FromEntity(mark.Subject) : null,
            Teacher = mark.Teacher?.User != null ? TeacherResponseDto.FromEntity(mark.Teacher, mark.Teacher.User) : null,
            Student = mark.Student?.User != null ? StudentResponseDto.FromEntity(mark.Student) : null,
            SemesterId = mark.SemesterId,
            CreatedAt = mark.CreatedAt,
            UpdatedAt = mark.UpdatedAt
            // Grade, GpaPoints, and Status are set set by the service layer with grading system
        };
    }
}
