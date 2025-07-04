using System.Security.Claims;
using System.Web;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniTrackRemaster.Api.Dto.Auth;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Services.Messaging;
using UniTrackRemaster.Services.Messaging.Enums;
using UniTrackRemaster.Services.Authentication;
using UniTrackRemaster.Services.User;

namespace UniTrackRemaster.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthService authService,
        ISmtpService smtpService,
        IConfiguration configuration,
        IUserService userService)
        : ControllerBase
    {
        /// <summary>
        /// Authenticates a user and provides a JWT and refresh token.
        /// </summary>
        /// <remarks>
        /// This endpoint authenticates the user based on the provided email and password.
        /// If authentication is successful, it returns a JWT for accessing protected resources and a refresh token.
        /// The tokens are set as cookies in the response.
        /// </remarks>
        /// <param name="model">The login credentials (email and password).</param>
        /// <response code="200">Successful login with JWT and refresh token.</response>
        /// <response code="401">Unauthorized if credentials are invalid.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var user = await authService.LoginUser(model);
                if (user is null)
                    return Unauthorized();

                var token = authService.GenerateJwtToken(user);
                var refreshToken = await authService.GenerateRefreshToken(user);
                var userRole = await authService.GetUserRole(user);
                
                var response = new LoginResponseDto(
                    Id: user.Id,
                    Email: user.Email,
                    FirstName: user.FirstName,
                    LastName: user.LastName,
                    Role: userRole,
                    user.IsLinked);
                
                Response.Cookies.Append("RefreshToken", refreshToken,
                    CookieOptionManager.GenerateRefreshCookieOptions());
                Response.Cookies.Append("AccessToken", token, CookieOptionManager.GenerateAccessCookieOptions());

                return Ok(response);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Registers a new user and provides a JWT and refresh token.
        /// </summary>
        /// <remarks>
        /// This endpoint registers a new user with the provided details.
        /// On successful registration, it returns a JWT and a refresh token.
        /// </remarks>
        /// <param name="model">Registration details including email, password, and name.</param>
        /// <response code="200">Successful registration with JWT and refresh token.</response>
        /// <response code="400">Bad request if registration fails (e.g., email already in use).</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest("User registration failed");
            try
            {
                var user = await authService.RegisterUser(model);

                var emailToken = await authService.GetEmailConfirmationToken(user);

                if (emailToken is null)
                    return BadRequest("User registration failed");

                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Auth",
                    new { userId = user.Id, token = HttpUtility.UrlEncode(emailToken) },
                    protocol: HttpContext.Request.Scheme);

                if (callbackUrl is null)
                    return BadRequest("User registration failed");

                if (user.Email != null)
                    await smtpService.SendEmailAsync(user.FirstName, user.LastName, user.Email, callbackUrl,
                        EmailTemplateType.Verification);

                var token = authService.GenerateJwtToken(user);
                var refreshToken = await authService.GenerateRefreshToken(user);
                

                return Ok(new { token, refreshToken });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("User registration failed");
            }
        }
        
        /// <summary>
        /// Registers a new user and provides a JWT and refresh token.
        /// This user will not be associated with any organization.
        /// </summary>
        /// <remarks>
        /// This endpoint registers a new user with the provided details.
        /// On successful registration, it returns a JWT and a refresh token.
        /// </remarks>
        /// <param name="model">Registration details including email, password, and name.</param>
        /// <response code="200">Successful registration with JWT and refresh token.</response>
        /// <response code="400">Bad request if registration fails (e.g., email already in use).</response>
        [HttpPost("register-guest")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterGuest(RegisterGuestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest("User registration failed");
            try
            {
                var user = await authService.RegisterGuest(model);

                var emailToken = await authService.GetEmailConfirmationToken(user);

                if (emailToken is null)
                    return BadRequest("User registration failed");

                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Auth",
                    new { userId = user.Id, token = HttpUtility.UrlEncode(emailToken) },
                    protocol: HttpContext.Request.Scheme);

                if (callbackUrl is null)
                    return BadRequest("User registration failed");

                if (user.Email != null)
                    await smtpService.SendEmailAsync(user.FirstName, user.LastName, user.Email, callbackUrl,
                        EmailTemplateType.Verification);

                var token = authService.GenerateJwtToken(user);
                var refreshToken = await authService.GenerateRefreshToken(user);

                return Ok(new { token, refreshToken });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("User registration failed");
            }
        }

        /// <summary>
        /// Refreshes the JWT using a refresh token.
        /// </summary>
        /// <remarks>
        /// This endpoint uses the refresh token provided in cookies to generate a new JWT and refresh token.
        /// If the refresh token is valid, it returns new tokens as cookies in the response.
        /// </remarks>
        /// <response code="200">Successful refresh with new JWT and refresh token.</response>
        /// <response code="400">Bad request if the refresh token is missing or invalid.</response>
        [HttpPost("refresh-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["RefreshToken"];
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return BadRequest("Refresh token is required");
                }

                var user = await authService.GetUserFromRefreshToken(refreshToken);

                if (user is null)
                {
                    return BadRequest("Invalid refresh token");
                }

                var newAccessToken = authService.GenerateJwtToken(user);
                var newRefreshToken = await authService.GenerateRefreshToken(user);

                Response.Cookies.Append("RefreshToken", newRefreshToken,
                    CookieOptionManager.GenerateRefreshCookieOptions());
                Response.Cookies.Append("AccessToken", newAccessToken,
                    CookieOptionManager.GenerateAccessCookieOptions());
                return Ok("Token refreshed");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Logs out the user by invalidating the refresh token.
        /// </summary>
        /// <remarks>
        /// This endpoint logs out the current user by invalidating the refresh token.
        /// It deletes the refresh token cookie from the response.
        /// </remarks>
        /// <response code="200">Successful logout.</response>
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var user = await authService.GetUserFromRefreshToken(refreshToken);
                if (user != null)
                    await authService.LogoutUser(user);
            }

            Response.Cookies.Delete("RefreshToken", new CookieOptions { Secure = true, HttpOnly = true });

            return Ok();
        }

        /// <summary>
        /// Confirms a user's email address.
        /// </summary>
        /// <remarks>
        /// This endpoint verifies a user's email address using a unique user ID and a token sent via email.
        /// It requires both the user ID and the token to confirm the email.
        /// </remarks>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="token">The token sent to the user's email for confirmation.</param>
        /// <response code="200">Email confirmed successfully.</response>
        /// <response code="400">Bad request if the user ID or token is missing or invalid.</response>
        /// <response code="404">User not found if the user ID does not match any existing user.</response>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("User ID and Token are required");
            }

            var user = await userService.GetUserById(Guid.Parse(userId));

            if (user == null) return BadRequest("Email could not be confirmed");
            var result = await authService.ConfirmEmail(user, token);

            if (result.Succeeded)
            {
                return Ok("Email confirmed successfully");
            }

            return BadRequest("Email could not be confirmed");
        }

        /// <summary>
        /// Initiates the password reset process for a user.
        /// </summary>
        /// <remarks>
        /// This endpoint is used when a user forgets their password and needs to reset it.
        /// It sends a password reset link to the user's email if the account with the given email exists and is confirmed.
        /// </remarks>
        /// <param name="email">The email address of the user who wants to reset their password.</param>
        /// <response code="200">Indicates that a password reset link has been sent if an account with the email exists.</response>
        /// <response code="400">Bad request if the email is missing or invalid.</response>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }

            var user = await authService.GetUserByEmail(email);
            if (user is null)
                return NotFound("If user with this email exists an email has been sent.");

            var token = await authService.GenerateForgottenPasswordLink(user);
    
            // Generate the frontend URL instead of backend route
            var resetUrl = $"{configuration["FrontendUrl"]}/reset-password?email={HttpUtility.UrlEncode(user.Email)}&token={HttpUtility.UrlEncode(token)}";
    
            if (resetUrl != null)
                await smtpService.SendEmailAsync(user.FirstName, user.LastName, user.Email!, resetUrl,
                    EmailTemplateType.ResetPassword);

            return Ok("If an account with this email exists, a password reset link has been sent.");
        }

        /// <summary>
        /// Changes the password for the authenticated user.
        /// </summary>
        /// <remarks>
        /// This endpoint allows an authenticated user to change their password.
        /// It requires the current password and the new password.
        /// </remarks>
        /// <param name="model">The model containing current password and new password.</param>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Bad request if the model is invalid or the current password is incorrect.</response>
        /// <response code="401">Unauthorized if the user is not authenticated.</response>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get the current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await userService.GetUserById(Guid.Parse(userId));
            if (user == null)
            {
                return Unauthorized();
            }

            // Change the password
            var result = await authService.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return Ok("Password changed successfully");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <remarks>
        /// This endpoint allows a user to reset their password using a token received in their email.
        /// It requires a valid token, the user's email, and the new password.
        /// </remarks>
        /// <param name="dto">The dto containing the email, token, and new password of the user.</param>
        /// <response code="200">Password has been reset successfully.</response>
        /// <response code="400">Bad request if the dto state is invalid or the request is invalid.</response>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (string.IsNullOrEmpty(dto.Token) || string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest("Token and email are required");
            }

            var result = await authService.ResetPassword(dto);
            if (result is { Succeeded: true })
            {
                return Ok("Password has been reset successfully");
            }

            return BadRequest("Error resetting password");
        }
    }
}
