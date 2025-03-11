using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;


public enum FacultyStatus
{
    [Display(Name="Active")]
    Active,
    [Display(Name="Inactive")]
    Inactive,
    [Display(Name="UnderReview")]
    UnderReview
}
