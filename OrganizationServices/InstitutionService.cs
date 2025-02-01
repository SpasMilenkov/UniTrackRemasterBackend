using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using StorageService;
using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Data.Models.Images;

namespace OrganizationServices;

public class InstitutionService : IInstitutionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFirebaseStorageService _storageService;
    private const string InstitutionsPath = "institutions";

    public InstitutionService(IUnitOfWork unitOfWork, IFirebaseStorageService storageService)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
    }

    public async Task<InstitutionDto> GetByIdAsync(Guid id)
    {
        var entity = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException($"Educational Institution with ID {id} not found.");

        // Get signed URLs for all images and logo
        var imageUrls = new List<string>();
        foreach (var image in entity.Images)
        {
            var signedUrl = await _storageService.CreateSignedUrl(image.Url);
            imageUrls.Add(signedUrl);
        }

        var logoUrl = !string.IsNullOrEmpty(entity.LogoUrl) 
            ? await _storageService.CreateSignedUrl(entity.LogoUrl)
            : string.Empty;

        return InstitutionDto.FromEntity(entity, imageUrls, logoUrl);
    }

    public async Task<List<InstitutionDto>> GetAllAsync()
    {
        var entities = await _unitOfWork.Institutions.GetAllAsync();
        var dtos = new List<InstitutionDto>();

        foreach (var entity in entities)
        {
            var imageUrls = new List<string>();
            foreach (var image in entity.Images)
            {
                var signedUrl = await _storageService.CreateSignedUrl(image.Url);
                imageUrls.Add(signedUrl);
            }

            var logoUrl = !string.IsNullOrEmpty(entity.LogoUrl)
                ? await _storageService.CreateSignedUrl(entity.LogoUrl)
                : string.Empty;

            dtos.Add(InstitutionDto.FromEntity(entity, imageUrls, logoUrl));
        }

        return dtos;
    }
    public async Task UpdateAsync(Guid id, UpdateInstitutionDto updateDto, IFormFile? logo = null,
        IEnumerable<IFormFile>? newImages = null)
    {
        var existingEntity = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (existingEntity == null)
            throw new NotFoundException($"Educational Institution with ID {id} not found.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Handle logo update
            if (logo != null)
            {
                // Delete old logo if exists
                if (!string.IsNullOrEmpty(existingEntity.LogoUrl))
                {
                    await _storageService.DeleteFileAsync(existingEntity.LogoUrl);
                }

                var logoPath = $"{InstitutionsPath}/{id}/logo/{logo.FileName}";
                existingEntity.LogoUrl = await _storageService.UploadFileAsync(logo);
            }

            // Handle new images
            if (newImages != null && newImages.Any())
            {
                var uploadedImages = await _storageService.UploadFilesAsync(newImages, $"{InstitutionsPath}/{id}/images");
                foreach (var imageUrl in uploadedImages)
                {
                    existingEntity.Images.Add(new Image() { Url = imageUrl, InstitutionId = id });
                }
            }

            updateDto.UpdateEntity(existingEntity);
            await _unitOfWork.Institutions.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<List<InstitutionDto>> GetInstitutionsByUserId(string userId)
    {
        var institutions = await _unitOfWork.Institutions.GetInstitutionsByUserIdAsync(userId);
        var dtos = new List<InstitutionDto>();

        foreach (var entity in institutions)
        {
            var imageUrls = new List<string>();
            foreach (var image in entity.Images)
            {
                var signedUrl = await _storageService.CreateSignedUrl(image.Url);
                imageUrls.Add(signedUrl);
            }

            var logoUrl = !string.IsNullOrEmpty(entity.LogoUrl)
                ? await _storageService.CreateSignedUrl(entity.LogoUrl)
                : string.Empty;

            dtos.Add(InstitutionDto.FromEntity(entity, imageUrls, logoUrl));
        }

        return dtos;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException($"Educational Institution with ID {id} not found.");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Delete logo
            if (!string.IsNullOrEmpty(entity.LogoUrl))
            {
                await _storageService.DeleteFileAsync(entity.LogoUrl);
            }

            // Delete all associated images
            foreach (var image in entity.Images)
            {
                await _storageService.DeleteFileAsync(image.Url);
            }

            await _unitOfWork.Institutions.DeleteAsync(entity.Id);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
    
}