using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum FocusArea
{
    [Display(Name = "Liberal Arts")]
    LiberalArts,
    [Display(Name = "STEM")]
    STEM,
    [Display(Name = "Business")]
    Business,
    [Display(Name = "Medical")]
    Medical,
    [Display(Name = "Law")]
    Law,
    [Display(Name = "Arts")]
    Arts,
    [Display(Name = "Technical")]
    Technical,
    [Display(Name = "Research")]
    Research
}
