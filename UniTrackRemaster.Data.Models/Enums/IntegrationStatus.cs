using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum IntegrationStatus
{
    [Display(Name="RequiresVerification")]
    RequiresVerification,
    [Display(Name="Verified")]
    Verified,
    [Display(Name="AdditionalDataSubmitted")]
    AdditionalDataSubmitted,
    [Display(Name="Denied")]
    Denied,
    [Display(Name="Active")]
    Active,
    [Display(Name="Inactive")]
    Inactive
}