using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;


public enum RecurrencePattern
{
    [Display(Name = "None")]
    None,
    [Display(Name = "Daily")]
    Daily,
    [Display(Name = "Weekly")]
    Weekly,
    [Display(Name = "Monthly")]
    Monthly,
    [Display(Name = "Yearly")]
    Yearly,
    [Display(Name = "Custom")]
    Custom
}
