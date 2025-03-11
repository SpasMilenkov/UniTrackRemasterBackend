using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum LocationType
{
    [Display(Name="Urban")]
    Urban,
    [Display(Name="Suburban")]
    Suburban,
    [Display(Name="Rural")]
    Rural
}