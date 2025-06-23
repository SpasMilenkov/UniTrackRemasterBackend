using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum AbsenceStatus
{
    [Display(Name = "Absent")]
    Absent,                     // Student was not in class
    
    [Display(Name = "Late")]
    Late,                       // Student arrived late
    
    [Display(Name = "ExcusedAbsence")]
    ExcusedAbsence,            // Absent with valid reason (sick, family emergency)
    
    [Display(Name = "UnexcusedAbsence")]
    UnexcusedAbsence,          // Absent without valid reason (skipping)
    
    [Display(Name = "RemoteLearning")]
    RemoteLearning,            // Present via online/remote
    
    [Display(Name = "EarlyDismissal")]
    EarlyDismissal             // Left class early (with permission)
}
