using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Services.Storage;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.University;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Services.Messaging;
using UniTrackRemaster.Services.Authentication;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Services.Organization;

public class UniversityService(
    IUnitOfWork unitOfWork,
    UserManager<ApplicationUser> userManager,
    IFirebaseStorageService firebaseStorage,
    ISmtpService smtpService,
    IImageService imageService,
    IPasswordGenerator passwordGenerator,
    ILogger<UniversityService> logger)
    : IUniversityService
{

    public async Task<Guid> InitUniversityAsync(InitUniversityDto universityData, IFormFile? logo, List<IFormFile> images)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            
            var university = await unitOfWork.Universities.InitUniversityAsync(universityData);
            
            // Generate random password for admin
            var password = passwordGenerator.GenerateSecurePassword();
            
            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = $"admin_{universityData.Id}",
                Email = university.Institution.Email,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsLinked = true,
                Institutions = new List<Institution>() {university.Institution},
                AvatarUrl = null
            };
            
            var result = await userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create admin user");
            }
            var user = await userManager.FindByEmailAsync(university.Institution.Email);

            // Add user to Admin role
            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleResult.Succeeded)
            {
                throw new Exception("Failed to assign admin role to user");
            }
            
            // Create admin record
            var admin = new Admin
            {
                InstitutionId = university.InstitutionId,
                Position = "System Administrator",
                Role = AdminRole.SystemAdmin,
                Status = ProfileStatus.Active,
                StartDate = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                UserId = user.Id
            };
            
            await unitOfWork.Admins.CreateAsync(admin);

            // Handle logo upload
            string? logoUrl = null;
            if (logo != null && logo.Length > 0)
            {
                using var stream = logo.OpenReadStream();
                var customPath = $"institutions/{universityData.Id}/logo/{logo.FileName}";
                logoUrl = await firebaseStorage.UploadFileAsync(stream, customPath);
                university.Institution.LogoUrl = logoUrl;
            }

            if (!string.IsNullOrEmpty(logoUrl))
            {
                await unitOfWork.Images.AddAsync(new Image
                {
                    Url = logoUrl,
                    InstitutionId = university.InstitutionId,
                });
            }

            // Handle additional images
            var fileUrls = new List<string>();
            foreach (var file in images)
            {
                if (file.Length <= 0) continue;
                var fileUrl = await firebaseStorage.UploadFileAsync(file);
                fileUrls.Add(fileUrl);
            }

            await imageService.AddBulkAsync(fileUrls, university.InstitutionId);

            await smtpService.SendAdminCredentialsEmailAsync(
                university.Institution.Email,
                adminUser.UserName,
                password
            );            
            
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            return university.InstitutionId;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
    
    /// <inheritdoc />
    public async Task<IEnumerable<UniversityResponseDto>> GetAllAsync(string? searchQuery = null)
    {
        var universities = await unitOfWork.Universities.GetAllAsync(searchQuery);
        return universities.Select(UniversityResponseDto.FromEntity);
    }

    /// <inheritdoc />
    public async Task<UniversityResponseDto> GetByIdAsync(Guid id)
    {
        var university = await unitOfWork.Universities.GetByIdAsync(id);
        if (university == null)
            throw new NotFoundException($"University with ID {id} not found");
            
        return UniversityResponseDto.FromEntity(university);
    }

    /// <inheritdoc />
    public async Task<UniversityResponseDto> GetByInstitutionIdAsync(Guid institutionId)
    {
        var university = await unitOfWork.Universities.GetByInstitutionIdAsync(institutionId);
        if (university == null)
            throw new NotFoundException($"University with institution ID {institutionId} not found");
            
        return UniversityResponseDto.FromEntity(university);
    }
    /// <inheritdoc />
    public async Task<UniversityResponseDto> UpdateAsync(Guid id, UpdateUniversityDto dto)
    {
        var university = await unitOfWork.Universities.GetByIdAsync(id);
        if (university == null)
            throw new NotFoundException($"University with ID {id} not found");

        try
        {
            await unitOfWork.BeginTransactionAsync();

            // Update only the properties that were provided
            if (dto.FocusAreas != null) university.FocusAreas = dto.FocusAreas;
            if (dto.UndergraduateCount.HasValue) university.UndergraduateCount = dto.UndergraduateCount.Value;
            if (dto.GraduateCount.HasValue) university.GraduateCount = dto.GraduateCount.Value;
            if (dto.AcceptanceRate.HasValue) university.AcceptanceRate = dto.AcceptanceRate.Value;
            if (dto.ResearchFunding.HasValue) university.ResearchFunding = dto.ResearchFunding.Value;
            if (dto.HasStudentHousing.HasValue) university.HasStudentHousing = dto.HasStudentHousing.Value;
            if (dto.Departments != null) university.Departments = dto.Departments;

            await unitOfWork.Universities.UpdateAsync(university);
            await unitOfWork.CommitAsync();

            // Retrieve the updated university with all its associations
            var updatedUniversity = await unitOfWork.Universities.GetByIdAsync(id);
            return UniversityResponseDto.FromEntity(updatedUniversity);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error occurred while updating university with ID {Id}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id)
    {
        var university = await unitOfWork.Universities.GetByIdAsync(id);
        if (university == null)
            throw new NotFoundException($"University with ID {id} not found");

        try
        {
            await unitOfWork.BeginTransactionAsync();

            // Check for related entities
            if (university.Faculties?.Any() == true)
                throw new ValidationException("Cannot delete university with associated faculties");

            await unitOfWork.Universities.DeleteAsync(id);
            await unitOfWork.CommitAsync();
        }
        catch (ValidationException)
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            logger.LogError(ex, "Error occurred while deleting university with ID {Id}", id);
            throw;
        }
    }
}
