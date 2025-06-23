using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Mark;

/// <summary>
/// Create mark DTO with grading system support
/// </summary>
public class CreateMarkDto
{
    /// <summary>Grade input (A, B, 5, etc.) - will be converted to 0-100 score using institution's grading system</summary>
    [Required(ErrorMessage = "Grade is required")]
    public string Grade { get; set; } = string.Empty;

    /// <summary>Alternative: Direct numerical score (0-100). If provided, takes precedence over Grade</summary>
    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
    public decimal? Score { get; set; }

    /// <summary>Topic or assignment name</summary>
    [Required(ErrorMessage = "Topic is required")]
    [StringLength(200, ErrorMessage = "Topic cannot exceed 200 characters")]
    public string Topic { get; set; } = string.Empty;

    /// <summary>Optional description of the mark</summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>Type of mark (Test, Assignment, Quiz, etc.)</summary>
    [Required(ErrorMessage = "Mark type is required")]
    public MarkType Type { get; set; }

    /// <summary>Subject ID</summary>
    [Required(ErrorMessage = "Subject ID is required")]
    public Guid SubjectId { get; set; }

    /// <summary>Teacher ID</summary>
    [Required(ErrorMessage = "Teacher ID is required")]
    public Guid TeacherId { get; set; }

    /// <summary>Student ID</summary>
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }

    /// <summary>Semester ID (optional - defaults to current active semester)</summary>
    public Guid? SemesterId { get; set; }
}