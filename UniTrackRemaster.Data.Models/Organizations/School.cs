using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Organizations;

public class School 
{
    #region Properties
    public Guid Id { get; set; }
    public int StudentCount { get; set; }
    public double StudentTeacherRatio { get; set; }
    public bool HasSpecialEducation { get; set; }
    public IList<string> ExtracurricularActivities { get; set; }
    public bool HasUniform { get; set; }
    public string[]? Programs { get; set; }
    #endregion

    #region NavigationProperties
    public Guid? SchoolReportId { get; set; }
    public SchoolReport? SchoolReport { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }

    #endregion
}