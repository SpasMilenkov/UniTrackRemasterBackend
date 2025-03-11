using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;


public enum AssignmentType
{
    [Display(Name ="Homework")]
    Homework,
    [Display(Name ="Quiz")]
    Quiz,
    [Display(Name ="Exam")]
    Exam,
    [Display(Name ="Project")]
    Project,
    [Display(Name ="Presentation")]
    Presentation,
    [Display(Name ="Paper")]
    Paper,
    [Display(Name ="Laboratory")]
    Laboratory
}
