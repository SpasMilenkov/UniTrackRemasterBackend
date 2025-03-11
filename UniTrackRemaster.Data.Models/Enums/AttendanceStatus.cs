using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum AttendanceStatus
{
    [Display(Name = "Present")]
    Present,
    [Display(Name = "Absent")]
    Absent,
    [Display(Name = "Late")]
    Late,
    [Display(Name = "ExcusedAbsence")]
    ExcusedAbsence,
    [Display(Name = "UnexcusedAbsence")]
    UnexcusedAbsence,
    [Display(Name = "RemoteLearning")]
    RemoteLearning
}