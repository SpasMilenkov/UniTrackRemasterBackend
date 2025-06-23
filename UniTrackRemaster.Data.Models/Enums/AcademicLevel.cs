using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum AcademicLevel
{
    [Display(Name = "Elementary" )]
    Elementary,
    [Display(Name = "MiddleSchool" )]
    MiddleSchool,
    [Display(Name = "HighSchool" )]
    HighSchool,
    [Display(Name = "Undergraduate" )]
    Undergraduate,
    [Display(Name = "Graduate" )]
    Graduate,
    [Display(Name = "Doctoral" )]
    Doctoral,
    [Display(Name = "   Professional" )]
    Professional
}