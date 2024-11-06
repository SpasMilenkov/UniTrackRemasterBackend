using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Organizations;

public class School: BaseEntity
{
    #region Properties

    public Guid Id { get; set; }
    [Length(0, 100)]
    public required string Name { get; set; }
    [Length(0, 255)]
    public string? Description { get; set; }
    public string? Motto { get; set; }
    public IntegrationStatus IntegrationStatus { get; set; }
    public string? Website { get; set; }
    public string? Type { get; set; }
    public string[]? Programs { get; set; }
    #endregion
    #region NavigationProperties
    
    public ICollection<Student>? Students { get; set; }
    public ICollection<Teacher>? Teachers { get; set; }
    public ICollection<Event>? Events { get; set; }
    public Guid? SchoolReportId { get; set; }
    public SchoolReport? SchoolReport { get; set; }
    public List<SchoolImage>? Images { get; set; }
    public required SchoolAddress Address { get; set; }
    #endregion
}