using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum MembershipStatus
{
    [Display(Name="Active")]
    Active,
    [Display(Name="Inactive")]
    Inactive,
    [Display(Name="Suspended")]
    Suspended,
    [Display(Name="Alumni")]
    Alumni
}
