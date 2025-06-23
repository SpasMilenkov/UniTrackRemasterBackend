using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Absence;

/// <summary>
/// CreateAbsenceDto with semester support
/// </summary>
public class CreateAbsenceDto
{
    /// <summary>Date of the absence</summary>
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }

    /// <summary>Attendance status</summary>
    [Required(ErrorMessage = "Status is required")]
    public AbsenceStatus Status { get; set; }

    /// <summary>Reason for the absence</summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }

    /// <summary>Whether the absence is excused</summary>
    public bool IsExcused { get; set; }

    /// <summary>Student ID</summary>
    [Required(ErrorMessage = "Student ID is required")]
    public Guid StudentId { get; set; }

    /// <summary>Teacher ID</summary>
    [Required(ErrorMessage = "Teacher ID is required")]
    public Guid TeacherId { get; set; }

    /// <summary>Subject ID (optional for general absences)</summary>
    public Guid? SubjectId { get; set; }

    /// <summary>Semester ID (optional - defaults to current active semester)</summary>
    public Guid? SemesterId { get; set; }
}