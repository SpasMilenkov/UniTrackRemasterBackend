using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum AccreditationType
{
    [Display(Name = "Regional")]
    Regional,
    [Display(Name = "National")]
    National,
    [Display(Name = "Programmatic")]
    Programmatic,
    [Display(Name = "International")]
    International
}
