using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;


public enum CourseType
{
    [Display(Name = "Required")]
    Required,
    [Display(Name = "Elective")]
    Elective,
    [Display(Name = "CoreCurriculum")]
    CoreCurriculum,
    [Display(Name = "Research")]
    Research,
    [Display(Name = "Practicum")]
    Practicum
}