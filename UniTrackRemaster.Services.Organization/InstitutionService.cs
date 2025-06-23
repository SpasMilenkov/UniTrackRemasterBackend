using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Images;

namespace UniTrackRemaster.Services.Organization;

public class InstitutionService : IInstitutionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFirebaseStorageService _storageService;
    private readonly ILogger<InstitutionService> _logger;
    private const string InstitutionsPath = "institutions";

    public InstitutionService(IUnitOfWork unitOfWork, IFirebaseStorageService storageService, ILogger<InstitutionService> logger)
    {
        _unitOfWork = unitOfWork;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<InstitutionDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving institution with ID: {InstitutionId}", id);
        
        try
        {
            // Start a single transaction for all database operations
            await _unitOfWork.BeginTransactionAsync();

            // Get institution with related data in a single transaction context
            var entity = await _unitOfWork.Institutions.GetByIdAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Institution not found with ID: {InstitutionId}", id);
                throw new NotFoundException($"Educational Institution with ID {id} not found.");
            }

            var school = await _unitOfWork.Schools.GetByInstitutionIdAsync(id);
            var university = await _unitOfWork.Universities.GetByInstitutionIdAsync(id);

            // Commit the transaction
            await _unitOfWork.CommitAsync();

            // Collect all URLs that need signing (images + logo)
            var urlsToSign = new List<string>();

            // Add image URLs
            if (entity.Images?.Any() == true)
            {
                urlsToSign.AddRange(entity.Images.Select(img => img.Url));
            }

            // Add logo URL if it exists
            if (!string.IsNullOrEmpty(entity.LogoUrl))
            {
                urlsToSign.Add(entity.LogoUrl);
            }

            // Generate all signed URLs in a single batch operation
            var signedUrls = urlsToSign.Any()
                ? await _storageService.CreateSignedUrlsBatch(urlsToSign)
                : new List<string>();

            // Separate image URLs and logo URL from the batch results
            var imageUrls = new List<string>();
            var logoUrl = string.Empty;

            if (signedUrls.Any())
            {
                var imageCount = entity.Images?.Count ?? 0;

                // First N URLs are for images
                imageUrls = signedUrls.Take(imageCount).ToList();

                // Last URL is for logo (if logo exists)
                if (!string.IsNullOrEmpty(entity.LogoUrl) && signedUrls.Count > imageCount)
                {
                    logoUrl = signedUrls[imageCount];
                }
            }

            _logger.LogDebug("Successfully retrieved institution: {InstitutionId}", id);

            // Return the appropriate DTO based on what type of institution it is
            if (school is not null)
            {
                return InstitutionDto.FromEntity(entity, imageUrls, logoUrl, school.Id, null);
            }

            if (university is not null)
            {
                return InstitutionDto.FromEntity(entity, imageUrls, logoUrl, null, university.Id);
            }

            return InstitutionDto.FromEntity(entity, imageUrls, logoUrl);
        }
        catch (NotFoundException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            // Rollback transaction on any error
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error retrieving institution: {InstitutionId}", id);
            throw;
        }
    }

    public async Task<List<InstitutionDto>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all institutions (legacy method)");
        
        try
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

            _logger.LogDebug("Successfully retrieved {Count} institutions", dtos.Count);
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving all institutions");
            throw;
        }
    }

    public async Task<PagedResult<InstitutionDto>> GetAllAsync(string? nameFilter = null, string? typeFilter = null, 
        string? locationFilter = null, string? integrationStatusFilter = null, string? accreditationsFilter = null, 
        int page = 1, int pageSize = 50)
    {
        _logger.LogInformation("Retrieving institutions with filters - Name: {Name}, Type: {Type}, Location: {Location}, IntegrationStatus: {IntegrationStatus}, Accreditations: {Accreditations}, Page: {Page}, PageSize: {PageSize}", 
            nameFilter, typeFilter, locationFilter, integrationStatusFilter, accreditationsFilter, page, pageSize);
        
        // Validate pagination parameters
        if (page < 1)
        {
            throw new ArgumentException("Page number must be greater than 0", nameof(page));
        }
        
        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100", nameof(pageSize));
        }
        
        try
        {
            var institutions = await _unitOfWork.Institutions.GetAllAsync(nameFilter, typeFilter, locationFilter, 
                integrationStatusFilter, accreditationsFilter, page, pageSize);
            var totalCount = await _unitOfWork.Institutions.GetTotalCountAsync(nameFilter, typeFilter, locationFilter, 
                integrationStatusFilter, accreditationsFilter);
            
            var institutionDtos = new List<InstitutionDto>();
            
            foreach (var entity in institutions)
            {
                var imageUrls = new List<string>();
                foreach (var image in entity.Images ?? new List<Image>())
                {
                    try
                    {
                        var signedUrl = await _storageService.CreateSignedUrl(image.Url);
                        imageUrls.Add(signedUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate signed URL for image: {ImageUrl} in institution: {InstitutionId}", 
                            image.Url, entity.Id);
                        // Continue processing other images
                    }
                }

                var logoUrl = string.Empty;
                if (!string.IsNullOrEmpty(entity.LogoUrl))
                {
                    try
                    {
                        logoUrl = await _storageService.CreateSignedUrl(entity.LogoUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate signed URL for logo: {LogoUrl} in institution: {InstitutionId}", 
                            entity.LogoUrl, entity.Id);
                        // Continue with empty logo URL
                    }
                }

                institutionDtos.Add(InstitutionDto.FromEntity(entity, imageUrls, logoUrl));
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} institutions (Page {Page} of {TotalPages})", 
                institutionDtos.Count, page, totalPages);
            
            return new PagedResult<InstitutionDto>
            {
                Items = institutionDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving institutions with filters");
            throw;
        }
    }

    public async Task UpdateAsync(Guid id, UpdateInstitutionDto updateDto, IFormFile? logo = null,
        IEnumerable<IFormFile>? newImages = null)
    {
        _logger.LogInformation("Updating institution: {InstitutionId}", id);
        
        var existingEntity = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (existingEntity == null)
        {
            _logger.LogWarning("Institution not found for update: {InstitutionId}", id);
            throw new NotFoundException($"Educational Institution with ID {id} not found.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Handle logo update
            if (logo != null)
            {
                // Delete old logo if exists
                if (!string.IsNullOrEmpty(existingEntity.LogoUrl))
                {
                    try
                    {
                        await _storageService.DeleteFileAsync(existingEntity.LogoUrl);
                        _logger.LogDebug("Deleted old logo for institution: {InstitutionId}", id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old logo for institution: {InstitutionId}", id);
                        // Continue with update even if old logo deletion fails
                    }
                }

                var logoPath = $"{InstitutionsPath}/{id}/logo/{logo.FileName}";
                existingEntity.LogoUrl = await _storageService.UploadFileAsync(logo);
                _logger.LogDebug("Uploaded new logo for institution: {InstitutionId}", id);
            }

            // Handle new images
            if (newImages != null && newImages.Any())
            {
                var uploadedImages = await _storageService.UploadFilesAsync(newImages, $"{InstitutionsPath}/{id}/images");
                foreach (var imageUrl in uploadedImages)
                {
                    existingEntity.Images.Add(new Image() { Url = imageUrl, InstitutionId = id });
                }
                _logger.LogDebug("Uploaded {Count} new images for institution: {InstitutionId}", uploadedImages.Count, id);
            }

            updateDto.UpdateEntity(existingEntity);
            await _unitOfWork.Institutions.UpdateAsync(existingEntity);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Successfully updated institution: {InstitutionId}", id);
        }
        catch (NotFoundException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error updating institution: {InstitutionId}", id);
            throw;
        }
    }

    public async Task<List<InstitutionDto>> GetInstitutionsByUserId(string userId)
    {
        _logger.LogInformation("Retrieving institutions for user: {UserId}", userId);
        
        try
        {
            var institutions = await _unitOfWork.Institutions.GetInstitutionsByUserIdAsync(userId);
            var dtos = new List<InstitutionDto>();

            foreach (var entity in institutions)
            {
                var imageUrls = new List<string>();
                foreach (var image in entity.Images ?? new List<Image>())
                {
                    try
                    {
                        var signedUrl = await _storageService.CreateSignedUrl(image.Url);
                        imageUrls.Add(signedUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate signed URL for image: {ImageUrl} in institution: {InstitutionId}", 
                            image.Url, entity.Id);
                        // Continue processing other images
                    }
                }

                var logoUrl = string.Empty;
                if (!string.IsNullOrEmpty(entity.LogoUrl))
                {
                    try
                    {
                        logoUrl = await _storageService.CreateSignedUrl(entity.LogoUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to generate signed URL for logo: {LogoUrl} in institution: {InstitutionId}", 
                            entity.LogoUrl, entity.Id);
                        // Continue with empty logo URL
                    }
                }

                dtos.Add(InstitutionDto.FromEntity(entity, imageUrls, logoUrl));
            }

            _logger.LogDebug("Successfully retrieved {Count} institutions for user: {UserId}", dtos.Count, userId);
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving institutions for user: {UserId}", userId);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting institution: {InstitutionId}", id);
        
        var entity = await _unitOfWork.Institutions.GetByIdAsync(id);
        if (entity == null)
        {
            _logger.LogWarning("Institution not found for deletion: {InstitutionId}", id);
            throw new NotFoundException($"Educational Institution with ID {id} not found.");
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Delete logo
            if (!string.IsNullOrEmpty(entity.LogoUrl))
            {
                try
                {
                    await _storageService.DeleteFileAsync(entity.LogoUrl);
                    _logger.LogDebug("Deleted logo for institution: {InstitutionId}", id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete logo for institution: {InstitutionId}", id);
                    // Continue with deletion even if logo deletion fails
                }
            }

            // Delete all associated images
            foreach (var image in entity.Images ?? new List<Image>())
            {
                try
                {
                    await _storageService.DeleteFileAsync(image.Url);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete image: {ImageUrl} for institution: {InstitutionId}", 
                        image.Url, id);
                    // Continue with deletion even if image deletion fails
                }
            }

            await _unitOfWork.Institutions.DeleteAsync(entity.Id);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Successfully deleted institution: {InstitutionId}", id);
        }
        catch (NotFoundException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error deleting institution: {InstitutionId}", id);
            throw;
        }
    }
}