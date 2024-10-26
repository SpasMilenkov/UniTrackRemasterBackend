namespace UniTrackRemaster.Data.Models.Analytics;

public class UniversityReport
{
    #region Properties

    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required DateTime From { get; set; }
    public required DateTime To { get; set; }
    public required string ShortDescription { get; set; }
    public required string DetailedDescription { get; set; }
    public required decimal NumericalRating { get; set; }

    #endregion
    #region NavigationProperties
    public ICollection<AcademicalGroupReport> FacultyReports { get; set; }

    #endregion
}