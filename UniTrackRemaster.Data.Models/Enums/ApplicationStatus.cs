using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ApplicationStatus
{
    [Display(Name = "Pending")]
    Pending,
    [Display(Name = "Approved")]
    Approved,
    [Display(Name = "Denied")]
    Denied,
    [Display(Name = "Verified")]
    Verified
}