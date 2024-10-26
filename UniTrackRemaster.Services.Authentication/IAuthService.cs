using Microsoft.AspNetCore.Identity;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Authentication;

public interface IAuthService
{
    public string GenerateJwtToken(ApplicationUser user);
    public Task<string> GenerateRefreshToken(ApplicationUser user);
    public Task<ApplicationUser?> GetUserFromRefreshToken(string refreshToken);
    public Task<string?> GetEmailConfirmationToken(ApplicationUser user);
    public Task<ApplicationUser> RegisterUser(RegisterDto model);
    public Task<ApplicationUser?> LoginUser(LoginDto model);
    public Task SignInUser(ApplicationUser user);
    public Task LogoutUser(ApplicationUser user);
    public Task<IdentityResult> ConfirmEmail(ApplicationUser user, string token);
    public Task<IdentityResult?> ResetPassword(ResetPasswordDto dto);
    public Task<string> GenerateForgottenPasswordLink(ApplicationUser user);
    public Task<ApplicationUser?> GetUserByEmail(string email);
    public Task<ApplicationUser?> GetUserById(string id);
    public Task<string> GetUserRole(ApplicationUser user); 
}