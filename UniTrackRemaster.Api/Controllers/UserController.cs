using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Services.Authentication;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController(IRoleService roleService) : ControllerBase
    {
        [HttpGet("current")]
        public ActionResult<string> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
            if (userId == null)
            {
                return Unauthorized();
            }

            return Ok(userId);
        }

        // If you need to get specific user details
        [HttpGet("{id}")]
        public ActionResult<string> GetUser(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            // Only allow access if the requested ID matches the current user's ID
            if (id != currentUserId)
            {
                return Forbid();
            }

            return Ok(currentUserId);
        }
    }
}
