using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum SubjectType
{
    [Display(Name = "SchoolSubject" )]
    SchoolSubject,      // Regular K-12 subject
    [Display(Name = "UniversityCourse" )]
    UniversityCourse,   // Higher education course
    [Display(Name = "VocationalSubject" )]
    VocationalSubject,  // For vocational training
    [Display(Name = "ElectiveCourse" )]
    ElectiveCourse      // Optional courses in either context
}