using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ClubType
{
    [Display(Name = "Academic")]
    Academic,
    [Display(Name = "Sports")]
    Sports,
    [Display(Name = "Arts")]
    Arts,
    [Display(Name = "Culture")]
    Culture,
    [Display(Name = "Technology")]
    Technology,
    [Display(Name = "Community")]
    Community,
    [Display(Name = "Professional")]
    Professional
}