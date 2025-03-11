using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum DepartmentStatus
{
    [Display(Name="Active")]
    Active,
    [Display(Name="Inactive")]
    Inactive,
    [Display(Name="UnderReview")]
    UnderReview,
    [Display(Name="Restructuring")]
    Restructuring
}
