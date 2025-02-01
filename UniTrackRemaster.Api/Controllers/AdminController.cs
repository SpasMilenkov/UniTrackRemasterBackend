using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Services.Admin;

namespace UniTrackRemaster.Controllers
{
    /// <summary>
    /// Controller for managing admin-related operations. Requires SuperAdmin role.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets all admin users in the system
        /// </summary>
        /// <returns>Collection of admin users</returns>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<IEnumerable<AdminDto>>> GetAll()
        {
            var admins = await _adminService.GetAllAsync();
            return Ok(admins);
        }
        
        /// <summary>
        /// Gets an admin by their unique identifier
        /// </summary>
        /// <param name="id">The admin's GUID</param>
        /// <returns>Admin details if found, 404 if not found</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<AdminDto>> GetById(Guid id)
        {
            var admin = await _adminService.GetByIdAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            return Ok(admin);
        }

        /// <summary>
        /// Gets all admins associated with a specific institution
        /// </summary>
        /// <param name="institutionId">The institution's GUID</param>
        /// <returns>Collection of admins belonging to the institution</returns>
        [HttpGet("institution/{institutionId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<AdminDto>>> GetByInstitution(Guid institutionId)
        {
            // Check if user belongs to this institution
            if (!User.IsInRole("SuperAdmin"))
            {
                var userInstitutionId = User.FindFirst("InstitutionId")?.Value;
                if (userInstitutionId != institutionId.ToString())
                {
                    return Forbid();
                }
            }

            var admins = await _adminService.GetByInstitutionAsync(institutionId);
            return Ok(admins);
        }

        /// <summary>
        /// Creates a new admin user
        /// </summary>
        /// <param name="createAdminDto">Admin creation details</param>
        /// <returns>Created admin details and location header, or 400 if creation fails</returns>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<AdminDto>> Create([FromBody] CreateAdminDto createAdminDto)
        {
            try
            {
                var admin = await _adminService.CreateAsync(createAdminDto);
                return CreatedAtAction(nameof(GetById), new { id = admin.Id }, admin);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Failed to create admin");
                return BadRequest(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Updates an existing admin's information
        /// </summary>
        /// <param name="id">The admin's GUID</param>
        /// <param name="updateAdminDto">Updated admin details</param>
        /// <returns>Updated admin information, 404 if not found, or 500 for server errors</returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<AdminDto>> Update(Guid id, [FromBody] UpdateAdminDto updateAdminDto)
        {
            try
            {
                var admin = await _adminService.UpdateAsync(id, updateAdminDto);
                return Ok(admin);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update admin");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Deletes an admin from the system
        /// </summary>
        /// <param name="id">The admin's GUID</param>
        /// <returns>204 if successful, 404 if not found, or 500 for server errors</returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _adminService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete admin");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
        
        /// <summary>
        /// Gets admin details by associated user ID
        /// </summary>
        /// <param name="userId">The user's GUID</param>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<AdminDto>> GetByUserId(Guid userId)
        {
            var admin = await _adminService.GetByUserIdAsync(userId);
            if (admin == null)
            {
                return NotFound();
            }

            return Ok(admin);
        }

    }
}
