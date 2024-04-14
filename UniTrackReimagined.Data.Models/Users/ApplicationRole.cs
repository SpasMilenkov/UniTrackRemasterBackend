using Microsoft.AspNetCore.Identity;

namespace UniTrackReimagined.Data.Models.Users;

public class ApplicationRole: IdentityRole<Guid>
{
    public ApplicationRole() : base()
    {
    }
    public ApplicationRole(string roleName) : base(roleName)
    {
        
    }
}