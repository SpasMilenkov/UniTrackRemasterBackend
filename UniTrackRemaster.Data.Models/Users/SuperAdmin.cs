namespace UniTrackRemaster.Data.Models.Users;

public class SuperAdmin: BaseEntity
{
    #region Properties

        public required Guid Id { get; set; }
    
    #endregion
    
    #region NavigationProperties
    
        public required Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }

    #endregion
}