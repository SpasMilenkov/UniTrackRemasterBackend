using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum SemesterStatus
{
    [Display(Name = "Upcoming")]
    Upcoming,
    [Display(Name = "Active")]
    Active,
    [Display(Name = "Completed")]
    Completed,
    [Display(Name = "Archived")]
    Archived,
    [Display(Name = "Cancelled")]
    Cancelled
}