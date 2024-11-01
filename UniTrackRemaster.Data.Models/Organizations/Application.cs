using UniTrackRemaster.Data.Models.Location;

namespace UniTrackRemaster.Data.Models.Organizations;

public class Application
{
    #region Propeties
    public required Guid Id { get; set; }

    public required string FirstName { get; set; }
    
    public required string LastName { get; set; }
    
    public required string Email { get; set; }
    
    public required string Phone { get; set; }

    
    #endregion

    #region NavigationProperties
    public School? School { get; set; }
    public required Guid SchoolId { get; set; }

    #endregion
}
