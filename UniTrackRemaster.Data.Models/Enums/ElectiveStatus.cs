using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ElectiveStatus
{
    [Display(Name="Enrolled")]
    Enrolled,
    [Display(Name="Completed")]
    Completed,
    [Display(Name="Dropped")]
    Dropped,
    [Display(Name="Waitlisted")]
    Waitlisted
}