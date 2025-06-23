using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Data.Models.Academical;

public class Semester : BaseEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
    
    // Existing relationships
    public Guid AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; }
    
    // Additional fields
    public SemesterType Type { get; set; }  // Fall, Spring, Summer, Winter
    public SemesterStatus Status { get; set; }  // Upcoming, Active, Completed, Archived
    public string? Description { get; set; }
    public int WeekCount { get; set; }  // Number of instructional weeks
    
    // Academic calendar dates
    public DateTime? RegistrationStartDate { get; set; }
    public DateTime? RegistrationEndDate { get; set; }
    public DateTime? AddDropDeadline { get; set; }
    public DateTime? WithdrawalDeadline { get; set; }
    public DateTime? MidtermStartDate { get; set; }
    public DateTime? MidtermEndDate { get; set; }
    public DateTime? FinalExamStartDate { get; set; }
    public DateTime? FinalExamEndDate { get; set; }
    public DateTime? GradeSubmissionDeadline { get; set; }
}
