using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ProfileStatus
{
    [Display(Name = "Pending")]
    Pending,      // Invitation sent, awaiting user acceptance

    [Display(Name = "Active")]
    Active,       // User accepted, profile is fully functional

    [Display(Name = "Rejected")]
    Rejected,     // User declined the invitation

    [Display(Name = "Inactive")]
    Inactive,     // Admin or user deactivated

    [Display(Name = "Suspended")]
    Suspended     // Temporarily disabled
}