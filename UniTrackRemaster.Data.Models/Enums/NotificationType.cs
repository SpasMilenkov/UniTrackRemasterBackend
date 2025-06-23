using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum NotificationType
{
    [Display(Name = "Invitation")]
    Invitation,
    [Display(Name = "Reminder")]
    Reminder,
    [Display(Name = "Update")]
    Update,
    [Display(Name = "Cancellation")]
    Cancellation,
    [Display(Name = "StartingSoon")]
    StartingSoon
}