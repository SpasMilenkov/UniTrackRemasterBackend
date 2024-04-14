using System.ComponentModel.DataAnnotations;
using UniTrackReimagined.Data.Models.Academical;
using UniTrackReimagined.Data.Models.Analytics;
using UniTrackReimagined.Data.Models.Events;
using UniTrackReimagined.Data.Models.Users;

namespace UniTrackReimagined.Data.Models.Organizations;

public class University
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