namespace UniTrackReimagined.Data.Models.Users;

public class SuperAdmin
{
    #region Properties

        public required Guid SuperAdminId { get; set; }
    
    #endregion
    
    #region NavigationProperties
    
        public required Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }

    #endregion
}