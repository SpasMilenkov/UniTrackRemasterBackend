using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum MeetingStatus
{
    [Display(Name="Scheduled")]
    Scheduled,
    [Display(Name="Completed")]
    Completed,
    [Display(Name="Cancelled")]
    Cancelled,
    [Display(Name="Rescheduled")]
    Rescheduled
}