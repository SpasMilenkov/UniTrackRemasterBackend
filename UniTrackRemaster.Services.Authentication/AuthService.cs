using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UniTrackRemaster.Api.Dto.Auth;
using UniTrackRemaster.Commons.Enums;
using UniTrackRemaster.Data.Context;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Authentication;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration config,
    UniTrackDbContext context,
    ILogger<AuthService> logger,
    RoleManager<ApplicationRole> roleManager)
    : IAuthService
{
    public string GenerateJwtToken(ApplicationUser user)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var roles = userManager.GetRolesAsync(user).Result;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };
            
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error generating JWT token");
            throw;
        }
    }
    
    public async Task<string> GenerateRefreshToken(ApplicationUser user)
    {
        try
        {
            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            user.RefreshToken = refreshToken;
            user.RefreshTokenValidity = DateTime.Now.AddHours(2).ToUniversalTime(); // Refresh token valid for 2 hours

            await userManager.UpdateAsync(user);

            return refreshToken;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while generating user refresh token");
            throw;
        }
    }

    public bool ValidateRefreshToken(ApplicationUser user, string refreshToken)
    {
        try
        {
            return user.RefreshToken == refreshToken && user.RefreshTokenValidity > DateTime.Now;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while authenticating the user");
            throw;
        }
    }
    public async Task<ApplicationUser?> GetUserFromRefreshToken(string refreshToken)
    {
        try
        {
            var user = await context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenValidity > DateTime.Now.ToUniversalTime());

            if (user != null)
            {
                return await userManager.FindByIdAsync(user.Id.ToString());
            }

            return null;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while attempting to get user from refresh token");
            throw;
        }

    }


    public async Task<string?> GetEmailConfirmationToken(ApplicationUser user)
    {
        try
        {
            return await userManager.GenerateEmailConfirmationTokenAsync(user);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while generating email confirmation token");
            throw;
        }
    }

    public async Task<ApplicationUser> RegisterUser(RegisterDto model)
    {
        try
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = false,
                AvatarUrl = "https://www.world-stroke.org/images/remote/https_secure.gravatar.com/avatar/9c62f39db51175255c24ef887c0b7101/"
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            if (!await roleManager.RoleExistsAsync(nameof(Roles.Guest)))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = nameof(Roles.Guest) });
            }

            await userManager.AddToRoleAsync(user, nameof(Roles.Guest));
            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error registering user");
            throw;
        }
    }
    
    public async Task<ApplicationUser> RegisterGuest(RegisterGuestDto model)
    {
        try
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = false,
                AvatarUrl = "https://www.world-stroke.org/images/remote/https_secure.gravatar.com/avatar/9c62f39db51175255c24ef887c0b7101/"
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"User creation failed: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            if (!await roleManager.RoleExistsAsync(nameof(Roles.Guest)))
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = nameof(Roles.Guest) });
            }

            await userManager.AddToRoleAsync(user, nameof(Roles.Guest));
            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error registering user");
            throw;
        }
    }


    public async Task<ApplicationUser?> LoginUser(LoginDto model)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user is null || !await userManager.CheckPasswordAsync(user, model.Password))
                return null;


            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while logging in the user");
            throw;
        }
    }
    public async Task LogoutUser(ApplicationUser user)
    {
        try
        {
            user.RefreshToken = null;
            await userManager.UpdateAsync(user);
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while logging out the user");
            throw;
        }
    }

    public async Task<IdentityResult> ConfirmEmail(ApplicationUser user, string token)
    {
        try
        {
            var result = await userManager.ConfirmEmailAsync(user, WebUtility.UrlDecode(token));
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while verifying the user email");
            throw;
        }
    }
    
    public async Task<IdentityResult?> ResetPassword(ResetPasswordDto dto)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user is null)
            {
                logger.LogWarning("ApplicationUser with that email does not exist", dto.Email);
                return null;
            }
            var result = await userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while resetting password");
            throw;
        }
    }

    public async Task<string> GenerateForgottenPasswordLink(ApplicationUser user)
    {
        try
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            if (token is null)
                throw new NullReferenceException();
            return token;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while generating password reset link");
            throw;
        }
    }

    public async Task<ApplicationUser?> GetUserByEmail(string email)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null )
            {
                logger.LogWarning("User with that email doesnt exist or has not confirmed it");
                return null;
            }

            if (await userManager.IsEmailConfirmedAsync(user)) return user;
            
            logger.LogWarning("User with that email has not confirmed it");
            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<ApplicationUser?> GetUserById(string id)
    {
        try
        {
            var user = await userManager.FindByIdAsync(id);
            if (user is null)
                logger.LogWarning("User with that id does not exist ${id}", id);
            return user;
        }
        catch (Exception e)
        {
            logger.LogError(e, "An error occurred while trying to fetch a user");
            throw;
        }
    }

    public async Task<string> GetUserRole(ApplicationUser user)
    {
        var roleList = await userManager.GetRolesAsync(user);
        var role = roleList.FirstOrDefault();
        return role is null ? nameof(Roles.Guest) :
            roleList.First();
    } 
}