using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;


public enum DepartmentType
{
    [Display(Name = "Academic" )]
    Academic,
    [Display(Name = "Research" )]
    Research,
    [Display(Name = "Administrative" )]
    Administrative,
    [Display(Name = "Support" )]
    Support
}