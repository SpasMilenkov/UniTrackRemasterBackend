using UniTrackRemaster.Data.Models.Analytics;

namespace UniTrackRemaster.Data.Models.Organizations;

public class School 
{
    #region Properties
    public Guid Id { get; set; }
    public int StudentCount { get; set; }
    public double StudentTeacherRatio { get; set; }
    public bool HasSpecialEducation { get; set; }
    public bool HasUniform { get; set; }
    public string[]? Programs { get; set; }
    #endregion

    #region NavigationProperties
    public Guid? SchoolReportId { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    #endregion
}