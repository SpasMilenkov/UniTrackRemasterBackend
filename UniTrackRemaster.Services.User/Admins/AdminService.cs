using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Admin;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Repositories;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;

namespace UniTrackRemaster.Services.User.Admins;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(
        IAdminRepository adminRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminService> logger,
        IUnitOfWork unitOfWork)
    {
        _adminRepository = adminRepository;
        _userManager = userManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<AdminDto>> GetAllAsync()
    {
        _logger.LogInformation("Getting all admins");
        try
        {
            var admins = await _adminRepository.GetAllAsync();
            return admins.Select(AdminDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all admins");
            throw;
        }
    }

    public async Task<AdminDto?> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Getting admin with ID: {AdminId}", id);
        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            return admin != null ? AdminDto.FromEntity(admin) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin with ID: {AdminId}", id);
            throw;
        }
    }

    public async Task<AdminDto?> GetByUserIdAsync(Guid userId)
    {
        _logger.LogInformation("Getting admin for user ID: {UserId}", userId);
        try
        {
            var admin = await _adminRepository.GetByUserIdAsync(userId);
            return admin != null ? AdminDto.FromEntity(admin) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admin for user ID: {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<AdminDto>> GetByInstitutionAsync(Guid institutionId)
    {
        _logger.LogInformation("Getting admins for institution ID: {InstitutionId}", institutionId);
        try
        {
            var admins = await _adminRepository.GetByInstitutionAsync(institutionId);
            return admins.Select(AdminDto.FromEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting admins for institution ID: {InstitutionId}", institutionId);
            throw;
        }
    }

    public async Task<AdminDto> CreateAsync(CreateAdminDto createAdminDto)
    {
        _logger.LogInformation("Creating admin with email: {Email}", createAdminDto.Email);

        // Validate input
        if (string.IsNullOrWhiteSpace(createAdminDto.Email))
            throw new ValidationException("Admin email is required");

        if (string.IsNullOrWhiteSpace(createAdminDto.FirstName))
            throw new ValidationException("Admin first name is required");

        if (string.IsNullOrWhiteSpace(createAdminDto.LastName))
            throw new ValidationException("Admin last name is required");

        if (string.IsNullOrWhiteSpace(createAdminDto.Position))
            throw new ValidationException("Admin position is required");

        // Begin transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Look up existing user
            var user = await _userManager.FindByEmailAsync(createAdminDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Application user with email {Email} not found", createAdminDto.Email);
                throw new NotFoundException($"Application user with email {createAdminDto.Email} not found. Please create a user account first.");
            }

            // Check if user is already an admin
            var existingAdmin = await _adminRepository.GetByUserIdAsync(user.Id);
            if (existingAdmin != null)
            {
                _logger.LogWarning("User with email {Email} is already registered as an admin", createAdminDto.Email);
                throw new InvalidOperationException($"User with email {createAdminDto.Email} is already registered as an admin.");
            }

            // Verify institution exists
            var institution = await _unitOfWork.Institutions.GetByIdAsync(createAdminDto.InstitutionId);
            if (institution == null)
            {
                _logger.LogWarning("Institution with ID {InstitutionId} not found", createAdminDto.InstitutionId);
                throw new NotFoundException($"Institution with ID {createAdminDto.InstitutionId} not found.");
            }

            // Add institution to user's institutions if not already added
            if (user.Institutions == null)
                user.Institutions = new List<Institution> { institution };
            else if (!user.Institutions.Any(i => i.Id == institution.Id))
                user.Institutions.Add(institution);

            // Mark user as linked to institution
            user.IsLinked = true;
            await _userManager.UpdateAsync(user);

            // Add user to Admin role if not already in that role
            if (!await _userManager.IsInRoleAsync(user, "Admin"))
                await _userManager.AddToRoleAsync(user, "Admin");

            // Create admin entity
            var admin = new Admin
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Position = createAdminDto.Position,
                Department = createAdminDto.Department,
                StartDate = DateTime.UtcNow,
                Role = createAdminDto.Role,
                Notes = null,
                InstitutionId = createAdminDto.InstitutionId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _adminRepository.CreateAsync(admin);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully created admin with ID: {AdminId}", admin.Id);
            return AdminDto.FromEntity(admin);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();

            // Rethrow specific exceptions for proper API responses
            if (ex is NotFoundException || ex is ValidationException || ex is InvalidOperationException)
            {
                throw;
            }

            _logger.LogError(ex, "Error creating admin: {Message}", ex.Message);
            throw new ApplicationException("An error occurred while creating the admin.", ex);
        }
    }

    public async Task<AdminDto> UpdateAsync(Guid id, UpdateAdminDto updateAdminDto)
    {
        _logger.LogInformation("Updating admin with ID: {AdminId}", id);
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
            {
                _logger.LogWarning("Admin with ID {AdminId} not found", id);
                throw new NotFoundException($"Admin with ID {id} not found");
            }

            // Update admin entity with DTO values
            updateAdminDto.UpdateEntity(admin);
            admin.UpdatedAt = DateTime.UtcNow;

            await _adminRepository.UpdateAsync(admin);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully updated admin with ID: {AdminId}", id);
            return AdminDto.FromEntity(admin);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();

            if (ex is NotFoundException)
                throw;

            _logger.LogError(ex, "Error updating admin with ID {AdminId}: {Message}", id, ex.Message);
            throw new ApplicationException($"An error occurred while updating admin with ID: {id}.", ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting admin with ID: {AdminId}", id);
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            if (admin == null)
            {
                _logger.LogWarning("Admin with ID {AdminId} not found", id);
                throw new NotFoundException($"Admin with ID {id} not found");
            }

            // Delete the admin record 
            await _adminRepository.DeleteAsync(id);

            // Do NOT delete the user - just remove from Admin role
            var user = await _userManager.FindByIdAsync(admin.UserId.ToString());
            if (user != null)
            {
                await _userManager.RemoveFromRoleAsync(user, "Admin");
                _logger.LogInformation("Removed Admin role from user {UserId}", admin.UserId);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Successfully deleted admin with ID: {AdminId}", id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();

            if (ex is NotFoundException)
                throw;

            _logger.LogError(ex, "Error deleting admin with ID {AdminId}: {Message}", id, ex.Message);
            throw new ApplicationException($"An error occurred while deleting admin with ID: {id}.", ex);
        }
    }
}