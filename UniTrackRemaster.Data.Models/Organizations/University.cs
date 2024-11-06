using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Analytics;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Organizations;

public class University: BaseEntity
{
    #region Properties

        public required Guid Id { get; set; }
        [Length(0, 100)]
        public required string Name { get; set; }
        [Length(0, 255)]
        public string? Description { get; set; }

    #endregion
    #region NavigationProperties
    
    public ICollection<Faculty>? Faculties { get; set; }
    public ICollection<Student>? Students { get; set; }
    public ICollection<Teacher>? Lecturers { get; set; }
    public ICollection<Event>? Events { get; set; }
    public required Guid UniversityReportId { get; set; }
    public UniversityReport? UniversityReport { get; set; }
    #endregion
}