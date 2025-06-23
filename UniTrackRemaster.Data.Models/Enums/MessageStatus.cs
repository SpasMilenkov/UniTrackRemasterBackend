using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum MessageStatus
{
    [Display(Name = "Sent")]
    Sent,
    [Display(Name = "Delivered")]
    Delivered,
    [Display(Name = "Edited")]
    Edited,
    [Display(Name = "Read")]
    Read,
    [Display(Name = "Deleted")]
    Deleted,
    [Display(Name = "Failed")]
    Failed
}