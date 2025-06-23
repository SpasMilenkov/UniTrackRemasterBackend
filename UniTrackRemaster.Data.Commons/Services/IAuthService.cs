using Microsoft.AspNetCore.Identity;
using UniTrackRemaster.Api.Dto.Auth;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Commons.Services;

public interface IAuthService
{
    public string GenerateJwtToken(ApplicationUser user);
    public Task<string> GenerateRefreshToken(ApplicationUser user);
    public Task<ApplicationUser?> GetUserFromRefreshToken(string refreshToken);
    public Task<string?> GetEmailConfirmationToken(ApplicationUser user);
    public Task<ApplicationUser> RegisterUser(RegisterDto model);
    public Task<ApplicationUser> RegisterGuest(RegisterGuestDto model);
    public Task<ApplicationUser?> LoginUser(LoginDto model);
    public Task LogoutUser(ApplicationUser user);
    public Task<IdentityResult> ConfirmEmail(ApplicationUser user, string token);
    public Task<IdentityResult?> ResetPassword(ResetPasswordDto dto);
    public Task<string> GenerateForgottenPasswordLink(ApplicationUser user);
    public Task<ApplicationUser?> GetUserByEmail(string email);
    Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    public Task<string> GetUserRole(ApplicationUser user); 
}