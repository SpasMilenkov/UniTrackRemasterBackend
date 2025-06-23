using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Mark;

/// <summary>
/// Update mark DTO with grading system support
/// </summary>
public class UpdateMarkDto
{
    /// <summary>Grade input (A, B, 5, etc.) - will be converted to 0-100 score using institution's grading system</summary>
    public string? Grade { get; set; }

    /// <summary>Alternative: Direct numerical score (0-100). If provided, takes precedence over Grade</summary>
    [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
    public decimal? Score { get; set; }

    /// <summary>Topic or assignment name</summary>
    [StringLength(200, ErrorMessage = "Topic cannot exceed 200 characters")]
    public string? Topic { get; set; }

    /// <summary>Optional description of the mark</summary>
    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    /// <summary>Type of mark (Test, Assignment, Quiz, etc.)</summary>
    public MarkType? Type { get; set; }
}