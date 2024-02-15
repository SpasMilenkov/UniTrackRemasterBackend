namespace UniTrackReimagined.Data.Models.Users;

public class Parent
{
    #region Properties

        public required Guid ParentId { get; set; }

    #endregion
    
    #region NavigationProperties
        public required Guid UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        public required ICollection<Student> Children {get; set;}

    #endregion
}