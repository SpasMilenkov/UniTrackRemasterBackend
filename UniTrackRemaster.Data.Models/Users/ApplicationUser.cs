using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UniTrackRemaster.Data.Models.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    
    #region Properties
        [Length(1, 50)]
        public string FirstName { get; set; }
        [Length(1, 50)]
        public string LastName { get; set; }
        public string? RefreshToken { get; set; } = null!;
        public string AvatarUrl {get; set;}
        public DateTime? RefreshTokenValidity { get; set; }
        public Guid SchoolId { get; set; }

    #endregion
}