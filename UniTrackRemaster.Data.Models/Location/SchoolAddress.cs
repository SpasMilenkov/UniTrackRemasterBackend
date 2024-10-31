using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Location;

public class SchoolAddress
{
    public Guid Id { get; set; }
    public string Country { get; set; }
    public string Settlement { get; set; }
    public string PostalCode { get; set; }
    public string Street { get; set; }
    public School School { get; set; }
    public Guid SchoolId { get; set; }
}