using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum ProfileVisibility
{
    [Display(Name = "Everyone")]
    Everyone,
    [Display(Name = "Institution")]
    Institution,
    [Display(Name = "None")]
    None
}