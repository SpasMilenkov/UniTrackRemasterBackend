using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UniTrackReimagined.Data.Models.Users;

public class ApplicationUser : IdentityUser
{
    #region Properties
        [Length(1, 50)]
        public required string FirstName { get; set; }
        [Length(1, 50)]
        public required string LastName { get; set; }

    #endregion
}