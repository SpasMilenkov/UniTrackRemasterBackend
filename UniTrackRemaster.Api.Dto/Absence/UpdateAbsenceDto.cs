using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Api.Dto.Absence;

/// <summary>
/// UpdateAbsenceDto
/// </summary>
public class UpdateAbsenceDto
{
    /// <summary>Date of the absence</summary>
    public DateTime? Date { get; set; }

    /// <summary>Attendance status</summary>
    public AbsenceStatus? Status { get; set; }

    /// <summary>Reason for the absence</summary>
    [StringLength(500, ErrorMessage = "Reason cannot exceed 500 characters")]
    public string? Reason { get; set; }

    /// <summary>Whether the absence is excused</summary>
    public bool? IsExcused { get; set; }
}