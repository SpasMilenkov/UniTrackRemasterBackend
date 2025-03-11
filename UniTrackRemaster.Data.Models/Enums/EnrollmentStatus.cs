using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;


public enum EnrollmentStatus
{
    [Display(Name="Enrolled")]
    Enrolled,
    [Display(Name="Dropped")]
    Dropped,
    [Display(Name="Completed")]
    Completed,
    [Display(Name="Failed")]
    Failed,
    [Display(Name="Withdrawn")]
    Withdrawn
}
