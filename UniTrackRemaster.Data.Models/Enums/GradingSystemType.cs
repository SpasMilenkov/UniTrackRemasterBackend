using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

/// <summary>
/// Predefined grading system types
/// </summary>
public enum GradingSystemType
{
    [Display(Name = "American")]
    American,    // A, B, C, D, F with +/- modifiers on 4.0 scale
    [Display(Name = "European")]
    European,    // ECTS system (A to F)
    [Display(Name = "Bulgarian")]
    Bulgarian,   // 2-6 scale
    [Display(Name = "Custom")]
    Custom       // Institution-specific custom system
}