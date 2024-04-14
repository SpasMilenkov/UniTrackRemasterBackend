using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UniTrackReimagined.Data.Models.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    #region Properties
        [Length(1, 50)]
        public string FirstName { get; set; }
        [Length(1, 50)]
        public string LastName { get; set; }

    #endregion
}