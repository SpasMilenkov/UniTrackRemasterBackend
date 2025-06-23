using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Role;
using UniTrackRemaster.Api.Dto.User;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Authentication;
using UniTrackRemaster.Services.User;

namespace UniTrackRemaster.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController(IRoleService roleService, IUserService userService, ILogger<UserController> logger) : ControllerBase
{
    [HttpGet("current")]
    public ActionResult<string> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return Unauthorized();

        return Ok(userId);
    }

    [HttpGet("{id}")]
    public ActionResult<string> GetUser(Guid id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == null) return Unauthorized();

        // Only allow access if the requested ID matches the current user's ID
        if (id != Guid.Parse(currentUserId)) return Forbid();

        return Ok(currentUserId);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDetailsResponse>> UpdateUserProfile(UpdateUserProfileDto profileDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return Unauthorized();

        try
        {
            var updatedUser = await userService.UpdateUserProfileAsync(Guid.Parse(userId), profileDto);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("details/{id}")]
    public async Task<ActionResult<UserDetailsResponse>> GetUserDetails(Guid id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == null) return Unauthorized();

        // Only allow access if the requested ID matches the current user's ID
        if (id != Guid.Parse(currentUserId)) return Forbid();

        try
        {
            var userDetails = await userService.GetUserDetailsAsync(id);
            return Ok(userDetails);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("profile-image")]
    public async Task<ActionResult<string>> UploadProfileImage(IFormFile image)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return Unauthorized();

        try
        {
            string imageUrl = await userService.UploadProfileImageAsync(Guid.Parse(userId), image);
            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("roles/institution/{institutionId:guid}")]
    public async Task<ActionResult<UserRolesResponse>> GetUserRolesInInstitution(Guid institutionId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return Unauthorized();

        try
        {
            var roles = await roleService.GetUserRolesInInstitutionAsync(Guid.Parse(userId), institutionId);
            return Ok(new UserRolesResponse(institutionId, roles));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("privacy")]
    public async Task<ActionResult<PrivacySettingsDto>> GetPrivacySettings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return Unauthorized();

        try
        {
            var privacySettings = await userService.GetPrivacySettingsAsync(Guid.Parse(userId));
            return Ok(privacySettings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("privacy")]
    public async Task<ActionResult<PrivacySettingsDto>> UpdatePrivacySettings(UpdatePrivacySettingsDto settingsDto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null) return Unauthorized();

        try
        {
            var updatedSettings = await userService.UpdatePrivacySettingsAsync(Guid.Parse(userId), settingsDto);
            return Ok(updatedSettings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("debug-claims")]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
    }

    [HttpGet("all")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<PaginatedUserListDto>> GetAllUsers([FromQuery] UserPaginationParams paginationParams)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        // Check if user is SuperAdmin (additional layer of security)
        var isSuperAdmin = await roleService.IsUserInRole(Guid.Parse(userId), "SuperAdmin");
        if (!isSuperAdmin) return Forbid();

        try
        {
            var paginatedUsers = await userService.GetAllUsersAsync(paginationParams);
            return Ok(paginatedUsers);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching paginated user list");
            return BadRequest(new { message = ex.Message });
        }
    }
}