using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum MarkType
{
    // Individual Assessment Types (existing)
    [Display(Name = "Written Examination")]
    WrittenExamination,
    [Display(Name = "Oral Examination")]
    OralExamination,
    [Display(Name = "Active Participation")]
    ActiveParticipation,
    [Display(Name = "Assignment")]
    Assignment,
    
    [Display(Name = "Quiz")]
    Quiz,
    [Display(Name = "Project")]
    Project,
    [Display(Name = "Lab Work")]
    LabWork,
    [Display(Name = "Presentation")]
    Presentation,
    [Display(Name = "Homework")]
    Homework,
    
    [Display(Name = "Quarter Grade")]
    QuarterGrade,
    [Display(Name = "Trimester Grade")]
    TrimesterGrade,
    [Display(Name = "Semester Grade")]
    SemesterGrade,
    [Display(Name = "Midterm Grade")]
    MidtermGrade,
    [Display(Name = "Final Grade")]
    FinalGrade,
    [Display(Name = "Annual Grade")]
    AnnualGrade,
    
    [Display(Name = "Makeup Exam")]
    MakeupExam,
    [Display(Name = "Extra Credit")]
    ExtraCredit,
    [Display(Name = "Incomplete")]
    Incomplete
}