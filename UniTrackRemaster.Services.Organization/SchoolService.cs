using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UniTrackRemaster.Services.Storage;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Services.Messaging;
using UniTrackRemaster.Services.Authentication;
using UniTrackRemaster.Data.Models.Enums;
using UniTrackRemaster.Services.Organization.Exceptions.Application;

namespace UniTrackRemaster.Services.Organization;

public class SchoolService(IUnitOfWork unitOfWork,
    IFirebaseStorageService firebaseStorage,
    IImageService imageService,
    ISmtpService smtpService,
    UserManager<ApplicationUser> userManager,
    IPasswordGenerator passwordGenerator)
    : ISchoolService
{
    public async Task<Guid> InitSchoolAsync(InitSchoolDto schoolData, IFormFile? logo, List<IFormFile> images)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var school = await unitOfWork.Schools.InitSchoolAsync(schoolData);

            // Generate random password for admin
            var password = passwordGenerator.GenerateSecurePassword();

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = $"admin_{schoolData.Id}",
                Email = school.Institution.Email,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsLinked = true,
                Institutions = new List<Institution>() { school.Institution },
                AvatarUrl = null
            };

            var result = await userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
            {
                throw new Exception("Failed to create admin user");
            }
            var user = await userManager.FindByEmailAsync(school.Institution.Email);

            var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
            if (!roleResult.Succeeded)
            {
                throw new Exception("Failed to assign admin role to user");
            }
            // Create admin record
            var admin = new Admin
            {
                InstitutionId = school.InstitutionId,
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
                var customPath = $"institutions/{schoolData.Id}/logo/{logo.FileName}";
                logoUrl = await firebaseStorage.UploadFileAsync(stream, customPath);
                school.Institution.LogoUrl = logoUrl;
            }

            if (!string.IsNullOrEmpty(logoUrl))
            {
                await unitOfWork.Images.AddAsync(new Image
                {
                    Url = logoUrl,
                    InstitutionId = school.InstitutionId,
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

            await imageService.AddBulkAsync(fileUrls, school.InstitutionId);

            await smtpService.SendAdminCredentialsEmailAsync(
                school.Institution.Email,
                adminUser.UserName,
                password
            );

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            return school.InstitutionId;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<string>> UploadSchoolImagesAsync(Guid schoolId, List<IFormFile> images)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            // First get the school to get its InstitutionId
            var school = await unitOfWork.Schools.GetByIdAsync(schoolId);
            var institutionId = school.InstitutionId;  // Use this for the images

            var uploadedUrls = new List<string>();

            if (images.Any())
            {
                var imageUrls = await firebaseStorage.UploadFilesAsync(
                    images,
                    $"institutions/{institutionId}/images"
                );
                uploadedUrls.AddRange(imageUrls);

                // Create image entities with the correct InstitutionId
                var imageEntities = imageUrls.Select(url => new Image
                {
                    Url = url,
                    InstitutionId = institutionId  // Use the institution ID, not the school ID
                }).ToList();

                await unitOfWork.Images.AddRangeAsync(imageEntities);
            }

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            return uploadedUrls;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<SchoolWithAddressResponseDto> GetSchoolAsync(Guid schoolId)
    {
        var school = await unitOfWork.Schools.GetByIdAsync(schoolId);

        // Get signed URLs for the institution's images
        var imageUrls = new List<string>();
        if (school.Institution.Images?.Any() == true)
        {
            var signedUrls = await Task.WhenAll(school.Institution.Images
                .Select(i => firebaseStorage.CreateSignedUrl(i.Url)));
            imageUrls.AddRange(signedUrls);
        }

        // Find logo (assuming it's marked specially in the images collection)
        var logo = school.Institution.Images?.FirstOrDefault(i => i.Url.Contains("/logo/"));
        string? signedLogoUrl = null;
        if (logo != null)
        {
            signedLogoUrl = await firebaseStorage.CreateSignedUrl(logo.Url);
        }

        return SchoolWithAddressResponseDto.FromEntity(
            school: school,
            signedLogoUrl: signedLogoUrl,
            signedUrls: imageUrls,
            address: school.Institution.Address);
    }

    public async Task<SchoolWithAddressResponseDto> GetSchoolByInstitutionIdAsync(Guid institutionId)
    {
        var school = await unitOfWork.Schools.GetByInstitutionIdAsync(institutionId);
    
        if (school == null)
        {
            throw new NotFoundException($"School not found for institution with ID {institutionId}");
        }

        // Get signed URLs for the institution's images
        var imageUrls = new List<string>();
        if (school.Institution.Images?.Any() == true)
        {
            var signedUrls = await Task.WhenAll(school.Institution.Images
                .Select(i => firebaseStorage.CreateSignedUrl(i.Url)));
            imageUrls.AddRange(signedUrls);
        }

        // Find logo (assuming it's marked specially in the images collection)
        var logo = school.Institution.Images?.FirstOrDefault(i => i.Url.Contains("/logo/"));
        string? signedLogoUrl = null;
        if (logo != null)
        {
            signedLogoUrl = await firebaseStorage.CreateSignedUrl(logo.Url);
        }

        return SchoolWithAddressResponseDto.FromEntity(
            school: school,
            signedLogoUrl: signedLogoUrl,
            signedUrls: imageUrls,
            address: school.Institution.Address);
    }
    
    public async Task<List<SchoolWithAddressResponseDto>> GetSchoolsAsync(SchoolFilterDto filter)
    {
        var schools = await unitOfWork.Schools.GetSchoolsAsync(filter);
        var schoolDtos = new List<SchoolWithAddressResponseDto>();

        foreach (var school in schools)
        {
            var imageUrls = new List<string>();
            if (school.Institution.Images?.Any() == true)
            {
                var signedUrls = await Task.WhenAll(school.Institution.Images
                    .Select(i => firebaseStorage.CreateSignedUrl(i.Url)));
                imageUrls.AddRange(signedUrls);
            }

            var logo = school.Institution.Images?.FirstOrDefault(i => i.Url.Contains("/logo/"));
            string? signedLogoUrl = null;
            if (logo != null)
            {
                signedLogoUrl = await firebaseStorage.CreateSignedUrl(logo.Url);
            }

            var schoolDto = SchoolWithAddressResponseDto.FromEntity(
                school: school,
                signedLogoUrl: signedLogoUrl,
                signedUrls: imageUrls,
                address: school.Institution.Address);
            schoolDtos.Add(schoolDto);
        }

        return schoolDtos;
    }

    public async Task<SchoolBaseResponseDto> UpdateSchoolAsync(UpdateSchoolDto updateDto)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var school = await unitOfWork.Schools.UpdateSchoolAsync(updateDto);

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            return SchoolBaseResponseDto.FromEntity(school);
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteSchoolAsync(Guid schoolId)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var school = await unitOfWork.Schools.GetByIdAsync(schoolId);
            if (school != null)
            {
                // Delete all institution images
                if (school.Institution.Images?.Any() == true)
                {
                    await Task.WhenAll(school.Institution.Images
                        .Select(i => firebaseStorage.DeleteFileAsync(i.Url)));
                }

                await unitOfWork.Schools.DeleteSchoolAsync(schoolId);
            }

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}