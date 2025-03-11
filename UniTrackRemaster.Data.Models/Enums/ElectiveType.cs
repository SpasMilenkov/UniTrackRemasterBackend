using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ElectiveType
{
    [Display(Name="Academic")]
    Academic,
    [Display(Name="Arts")]
    Arts,
    [Display(Name="Sports")]
    Sports,
    [Display(Name="Technology")]
    Technology,
    [Display(Name="Language")]
    Language,
    [Display(Name="Professional")]
    Professional
}