using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Services.Authentication;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IRoleService roleService) : ControllerBase
    {
        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await roleService.GetAllRolesAsync();
            return Ok(roles.Select((role) => role.Name != null ? new RoleResponseDto(role.Id, role.Name) : null));
        }

        [HttpGet("public")]
        public async Task<IActionResult> GetAllPublicUsers()
        {
            var roles = await roleService.GetPublicRolesAsync();
            
            return Ok(roles.Select((role) => role.Name != null ? new RoleResponseDto(role.Id, role.Name) : null));
        }
    }
}
