using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using StorageService;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Images;
using UniTrackRemaster.Data.Models.Users;
using UniTrackRemaster.Messaging;
using UniTrackRemaster.Services.Authentication;

namespace OrganizationServices;

public class UniversityService : IUniversityService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IFirebaseStorageService firebaseStorage;
    private readonly ISmtpService smtpService;
    private readonly IImageService ImageService;
    private readonly IPasswordGenerator _passwordGenerator;
    
    public UniversityService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IFirebaseStorageService firebaseStorage,
        ISmtpService smtpService,
        IImageService imageService,
        IPasswordGenerator passwordGenerator)
    {
        this.unitOfWork = unitOfWork;
        this.userManager = userManager;
        this.firebaseStorage = firebaseStorage;
        this.smtpService = smtpService;
        this.ImageService = imageService;
        this._passwordGenerator = passwordGenerator;
    }

    public async Task<Guid> InitUniversityAsync(InitUniversityDto universityData, IFormFile? logo, List<IFormFile> images)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();
            
            var university = await unitOfWork.Universities.InitUniversityAsync(universityData);
            
            // Generate random password for admin
            var password = _passwordGenerator.GenerateSecurePassword();
            
            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = $"admin_{universityData.Id}",
                Email = university.Institution.Email,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsLinked = true,
                AvatarUrl = "https://t3.ftcdn.net/jpg/00/64/67/52/360_F_64675209_7ve2XQANuzuHjMZXP3aIYIpsDKEbF5dD.jpg"
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
                Status = AdminStatus.Active,
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

            await ImageService.AddBulkAsync(fileUrls, university.InstitutionId);

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
}
