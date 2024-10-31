using UniTrackRemaster.Data.Models.Location;

namespace UniTrackRemaster.Data.Models.Organizations;

public class Application
{
    public required Guid Id { get; set; }

    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public required string Email { get; set; }
    
    public required string Phone { get; set; }

    public SchoolAddress Address { get; set; }
}
