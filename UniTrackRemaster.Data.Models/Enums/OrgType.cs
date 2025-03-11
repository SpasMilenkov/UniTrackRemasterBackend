using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum OrgType
{
    [Display(Name = "School")]
    School,
    [Display(Name = "University")]
    University
}