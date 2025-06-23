using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum SemesterType
{
    [Display(Name = "Fall")]
    Fall,
    [Display(Name = "Spring")]
    Spring,
    [Display(Name = "Summer")]
    Summer,
    [Display(Name = "Winter")]
    Winter,
    
    // Add K-12 term support
    [Display(Name = "First Quarter")]
    FirstQuarter,
    [Display(Name = "Second Quarter")]
    SecondQuarter,
    [Display(Name = "Third Quarter")]
    ThirdQuarter,
    [Display(Name = "Fourth Quarter")]
    FourthQuarter,
    
    [Display(Name = "First Trimester")]
    FirstTrimester,
    [Display(Name = "Second Trimester")]
    SecondTrimester,
    [Display(Name = "Third Trimester")]
    ThirdTrimester,
    
    [Display(Name = "Other")]
    Other
}