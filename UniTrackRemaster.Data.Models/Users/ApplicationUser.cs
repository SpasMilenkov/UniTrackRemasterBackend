using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Data.Models.Organizations;

namespace UniTrackRemaster.Data.Models.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    #region Properties
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public string? AvatarUrl {get; set;}
        public DateTime? RefreshTokenValidity { get; set; }
        //Indicates where the account is linked to an integrated institution
        //If not we treat it as a community account
        public bool IsLinked {get; set;}

        // Privacy settings
        public bool DataAnalytics { get; set; } = true;
        public bool EmailUpdates { get; set; } = true;
        public bool MarketingEmails { get; set; } = false;
        public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.Everyone;
        #endregion
        
        
    #region Navigation Properties
    public IList<Institution>? Institutions { get; set; }
    #endregion
}