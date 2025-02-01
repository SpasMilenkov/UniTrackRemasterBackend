using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    
    #region Properties
        [Length(1, 50)]
        public required string FirstName { get; set; }
        [Length(1, 50)]
        public required string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public required string AvatarUrl {get; set;}
        public DateTime? RefreshTokenValidity { get; set; }
        //Indicates where the account is linked to an integrated institution
        //If not we treat it as a community account
        public bool IsLinked {get; set;}

        #endregion
}   