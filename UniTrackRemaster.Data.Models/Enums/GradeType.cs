using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Data.Models.Enums;

public enum GradeType
{
        [Display(Name = "WrittenExamination")]
        WrittenExamination,
        [Display(Name = "OralExamination")]
        OralExamination,
        [Display(Name = "ActiveParticipation")]
        ActiveParticipation,
        [Display(Name = "Assignment")]
        Assignment
}