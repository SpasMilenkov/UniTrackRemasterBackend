using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.Admin;

public class AdminService(
    IAdminRepository adminRepository,
    UserManager<ApplicationUser> userManager,
    ILogger<AdminService> logger)
    : IAdminService
{
    private readonly ILogger<AdminService> _logger = logger;

    public async Task<IEnumerable<AdminDto>> GetAllAsync()
    {
        var admins = await adminRepository.GetAllAsync();
        return admins.Select(AdminDto.FromEntity);
    }

    public async Task<AdminDto?> GetByIdAsync(Guid id)
    {
        var admin = await adminRepository.GetByIdAsync(id);
        return admin != null ? AdminDto.FromEntity(admin) : null;
    }

    public async Task<AdminDto?> GetByUserIdAsync(Guid userId)
    {
        var admin = await adminRepository.GetByUserIdAsync(userId);
        return admin != null ? AdminDto.FromEntity(admin) : null;
    }

    public async Task<IEnumerable<AdminDto>> GetByInstitutionAsync(Guid institutionId)
    {
        var admins = await adminRepository.GetByInstitutionAsync(institutionId);
        return admins.Select(AdminDto.FromEntity);
    }

    public async Task<AdminDto> CreateAsync(CreateAdminDto createAdminDto)
    {
        var user = new ApplicationUser
        {
            UserName = createAdminDto.Email,
            Email = createAdminDto.Email,
            FirstName = createAdminDto.FirstName,
            LastName = createAdminDto.LastName,
            AvatarUrl = null
        };

        var result = await userManager.CreateAsync(user, createAdminDto.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, "Admin");

        var admin = CreateAdminDto.ToEntity(createAdminDto, user.Id);
        await adminRepository.CreateAsync(admin);
        
        return AdminDto.FromEntity(admin);
    }

    public async Task<AdminDto> UpdateAsync(Guid id, UpdateAdminDto updateAdminDto)
    {
        var admin = await adminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            throw new KeyNotFoundException($"Admin with ID {id} not found");
        }

        updateAdminDto.UpdateEntity(admin);
        await adminRepository.UpdateAsync(admin);
        
        return AdminDto.FromEntity(admin);
    }

    public async Task DeleteAsync(Guid id)
    {
        var admin = await adminRepository.GetByIdAsync(id);
        if (admin == null)
        {
            throw new KeyNotFoundException($"Admin with ID {id} not found");
        }

        var user = await userManager.FindByIdAsync(admin.UserId.ToString());
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }

        await adminRepository.DeleteAsync(id);
    }
}