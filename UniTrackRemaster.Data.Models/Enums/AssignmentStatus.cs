using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;


public enum AssignmentStatus
{
    [Display(Name ="Pending")]
    Pending,
    [Display(Name ="Submitted")]
    Submitted,
    [Display(Name ="Late")]
    Late,
    [Display(Name ="Graded")]
    Graded,
    [Display(Name ="Missing")]
    Missing
}