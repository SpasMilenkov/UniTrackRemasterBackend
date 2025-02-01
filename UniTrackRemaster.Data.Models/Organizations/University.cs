using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Organizations;

public class University 
{
    #region Properties
    public Guid Id { get; set; }
    public IList<FocusArea> FocusAreas { get; set; }
    public int UndergraduateCount { get; set; }
    public int GraduateCount { get; set; }
    public double AcceptanceRate { get; set; }
    public int ResearchFunding { get; set; }
    public bool HasStudentHousing { get; set; }
    public IList<string> Departments { get; set; }
    #endregion
    
    #region NavigationProperties
    public IList<Faculty>? Faculties { get; set; }
    public Guid UniversityReportId { get; set; }
    public UniversityReport? UniversityReport { get; set; }
    public Guid InstitutionId { get; set; }
    public Institution? Institution { get; set; }
    #endregion
}
