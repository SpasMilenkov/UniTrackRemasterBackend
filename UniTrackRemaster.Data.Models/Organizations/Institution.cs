using System.ComponentModel.DataAnnotations;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Events;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.Location;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Data.Models.Organizations;

public class Institution : BaseEntity
{
    public Guid Id { get; set; }
    [Length(0, 100)]
    public required string Name { get; set; }
    [Length(0, 255)]
    public string? Description { get; set; }
    public InstitutionType Type { get; set; }
    public LocationType Location { get; set; }
    public IList<AccreditationType> Accreditations { get; set; } = new List<AccreditationType>();
    public required Address Address { get; set; }
    public IList<Image> Images { get; set; } = new List<Image>();
    public string? LogoUrl { get; set; }
    public DateTime EstablishedDate { get; set; }
    public string? Website { get; set; }    
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public string? Motto { get; set; }
    public IntegrationStatus IntegrationStatus { get; set; }
    public IList<Student>? Students { get; set; }
    public IList<Teacher>? Teachers { get; set; }
    public IList<Admin>? Admins { get; set; } 
    public IList<Parent>? Parents { get; set; } 
    public IList<Event>? Events { get; set; }
    public IList<Subject>? Subjects { get; set; }
}