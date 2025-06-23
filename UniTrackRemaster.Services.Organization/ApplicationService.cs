using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Address;
using UniTrackRemaster.Api.Dto.Application;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Commons.Services;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Services.Messaging;
using UniTrackRemaster.Services.Messaging.Enums;
using UniTrackRemaster.Services.Organization.Exceptions.Application;
using ApplicationException = UniTrackRemaster.Services.Organization.Exceptions.Application.ApplicationException;

namespace UniTrackRemaster.Services.Organization;

public class ApplicationService : IApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISmtpService _smtpService;
    private readonly ILogger<ApplicationService> _logger;
    private readonly IGradingSystemService _gradingSystemService;

    public ApplicationService(
        IUnitOfWork unitOfWork, 
        ISmtpService smtpService,
        ILogger<ApplicationService> logger,
        IGradingSystemService gradingSystemService)
    {
        _unitOfWork = unitOfWork;
        _smtpService = smtpService;
        _logger = logger;
        _gradingSystemService = gradingSystemService;
    }

    public async Task<ApplicationResponseDto> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving application with ID: {ApplicationId}", id);
        
        try
        {
            var application = await _unitOfWork.Applications.GetByIdAsync(id);
            
            if (application == null)
            {
                _logger.LogWarning("Application not found with ID: {ApplicationId}", id);
                throw new NotFoundException("Application", id.ToString());
            }
            
            if (application.Institution == null)
            {
                _logger.LogError("Application {ApplicationId} exists but has no associated institution", id);
                throw new BusinessRuleViolationException("DataIntegrity", 
                    $"Application {id} exists but has no associated institution");
            }
            
            _logger.LogDebug("Successfully retrieved application: {ApplicationId}", id);
            return ApplicationResponseDto.FromEntity(
                application, 
                AddressDto.FromEntity(application.Institution.Address));
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            _logger.LogError(ex, "Unexpected error retrieving application: {ApplicationId}", id);
            throw;
        }
    }

    public async Task<ApplicationResponseDto> GetByCodeAsync(string code, string email)
    {
        _logger.LogInformation("Verifying code for email: {Email}", email);
        
        try
        {
            var application = await _unitOfWork.Applications.VerifyCodeAsync(email, code);
            
            if (application.Institution == null)
            {
                _logger.LogError("Application {ApplicationId} exists but has no associated institution", application.Id);
                throw new BusinessRuleViolationException("DataIntegrity", 
                    $"Application {application.Id} exists but has no associated institution");
            }
            
            _logger.LogInformation("Successfully verified code for application: {ApplicationId}", application.Id);
            return ApplicationResponseDto.FromEntity(
                application, 
                AddressDto.FromEntity(application.Institution.Address));
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            _logger.LogError(ex, "Unexpected error during code verification for email: {Email}", email);
            throw;
        }
    }

    public async Task<ApplicationResponseDto> GetByInstitutionIdAsync(Guid id)
    {
        _logger.LogInformation("Retrieving application for institution: {InstitutionId}", id);
        
        try
        {
            var application = await _unitOfWork.Applications.GetByInstitutionIdAsync(id);
            
            if (application == null)
            {
                _logger.LogWarning("No application found for institution: {InstitutionId}", id);
                throw new NotFoundException("Application", $"institution ID: {id}");
            }

            if (application.Institution == null)
            {
                _logger.LogError("Application {ApplicationId} exists but has no associated institution", application.Id);
                throw new BusinessRuleViolationException("DataIntegrity", 
                    $"Application {application.Id} exists but has no associated institution");
            }

            _logger.LogDebug("Successfully retrieved application for institution: {InstitutionId}", id);
            return ApplicationResponseDto.FromEntity(
                application,
                AddressDto.FromEntity(application.Institution.Address));
        }
        catch (Exception ex) when (ex is not ApplicationException)
        {
            _logger.LogError(ex, "Unexpected error retrieving application for institution: {InstitutionId}", id);
            throw;
        }
    }
    
    public async Task<PagedResult<ApplicationResponseDto>> GetAllAsync(string? statusFilter = null, int page = 1, int pageSize = 50)
    {
        _logger.LogInformation("Retrieving applications with filter: {StatusFilter}, Page: {Page}, PageSize: {PageSize}", 
            statusFilter, page, pageSize);
        
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
            var applications = await _unitOfWork.Applications.GetAllAsync(statusFilter, page, pageSize);
            var totalCount = await _unitOfWork.Applications.GetTotalCountAsync(statusFilter);
            
            var applicationDtos = applications
                .Where(a => a.Institution != null)
                .Select(a => ApplicationResponseDto.FromEntity(
                    a, 
                    AddressDto.FromEntity(a.Institution!.Address)))
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            
            _logger.LogDebug("Successfully retrieved {Count} applications (Page {Page} of {TotalPages})", 
                applicationDtos.Count, page, totalPages);
            
            return new PagedResult<ApplicationResponseDto>
            {
                Items = applicationDtos,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalCount = totalCount,
                PageSize = pageSize
            };
        }
        catch (Exception ex) when (ex is not ApplicationException and not ArgumentException)
        {
            _logger.LogError(ex, "Unexpected error retrieving applications");
            throw;
        }
    }

    public async Task ApproveAsync(Guid id)
    {
        _logger.LogInformation("Attempting to approve application: {ApplicationId}", id);
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var application = await _unitOfWork.Applications.ApproveAsync(id);
            
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Successfully approved application: {ApplicationId}", id);
            
            // Send approval email (after successful commit)
            try
            {
                await _smtpService.SendEmailWithCodeAsync(
                    application.FirstName,
                    application.LastName,
                    application.Email,
                    application.Code,
                    EmailTemplateType.ApplicationApproved
                );
                
                _logger.LogInformation("Approval email sent for application: {ApplicationId}", id);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send approval email for application: {ApplicationId}", id);
                // Don't throw here - the approval was successful, email is secondary
            }
        }
        catch (ApplicationException)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogWarning("Failed to approve application: {ApplicationId}", id);
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error approving application: {ApplicationId}", id);
            throw;
        }
    }

    public async Task<ApplicationResponseDto> CreateAsync(CreateInstitutionApplicationDto applicationDto)
    {
        _logger.LogInformation("Creating application for institution: {InstitutionName}, Email: {Email}", 
            applicationDto.InstitutionName, applicationDto.Email);
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            // Check for existing application (this will throw DuplicateApplicationException if exists)
            // No need to manually check since the repository handles this
            
            // Create institution
            var institution = await _unitOfWork.Institutions.AddAsync(new Institution()
            {
                Name = applicationDto.InstitutionName,
                Type = applicationDto.InstitutionType,
                Address = AddressDto.ToEntity(applicationDto.Address),
                Email = applicationDto.Email,
                Phone = applicationDto.Phone
            });
        
            // Create application (this will throw DuplicateApplicationException if email exists)
            var applicationEntity = await _unitOfWork.Applications.CreateAsync(
                CreateInstitutionApplicationDto.ToEntity(applicationDto, institution.Id));
            
            // IMPORTANT: Initialize default grading systems for the new institution
            // This uses the transaction-safe method that works within our existing transaction
            var gradingSystemsCreated = await _gradingSystemService.InitializeDefaultGradingSystemsWithinTransactionAsync(institution.Id);
            
            if (gradingSystemsCreated)
            {
                _logger.LogInformation("Created default grading systems for new institution: {InstitutionId}", institution.Id);
            }
            else
            {
                _logger.LogDebug("Institution {InstitutionId} already had grading systems", institution.Id);
            }
            
            // Reload with full data
            var createdApplication = await _unitOfWork.Applications.GetByIdAsync(applicationEntity.Id);
            
            if (createdApplication?.Institution == null)
            {
                throw new BusinessRuleViolationException("DataIntegrity", 
                    "Failed to create application with proper institution association");
            }

            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Successfully created application: {ApplicationId} with default grading systems", createdApplication.Id);
            
            // Send creation email (after successful commit)
            try
            {
                await _smtpService.SendEmailWithCodeAsync(
                    applicationDto.FirstName,
                    applicationDto.LastName,
                    applicationDto.Email,
                    createdApplication.Code,
                    EmailTemplateType.ApplicationCreated
                );
                
                _logger.LogInformation("Creation email sent for application: {ApplicationId}", createdApplication.Id);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send creation email for application: {ApplicationId}", 
                    createdApplication.Id);
                throw new EmailDeliveryException(applicationDto.Email, "creation", emailEx);
            }
        
            return ApplicationResponseDto.FromEntity(
                createdApplication, 
                AddressDto.FromEntity(createdApplication.Institution.Address));
        }
        catch (ApplicationException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error creating application for: {Email}", applicationDto.Email);
            throw;
        }
    }

    public async Task<ApplicationResponseDto> UpdateAsync(Guid id, UpdateInstitutionApplicationDto updatedApplicationDto)
    {
        _logger.LogInformation("Updating application: {ApplicationId}", id);
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var application = await _unitOfWork.Applications.UpdateAsync(id,
                UpdateInstitutionApplicationDto.ToEntity(updatedApplicationDto));
        
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Successfully updated application: {ApplicationId}", id);
        
            return ApplicationResponseDto.FromEntity(
                application, 
                AddressDto.FromEntity(application.Institution!.Address));
        }
        catch (ApplicationException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error updating application: {ApplicationId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting application: {ApplicationId}", id);
        
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var application = await _unitOfWork.Applications.GetByIdAsync(id);
            if (application == null)
            {
                _logger.LogWarning("Application not found for deletion: {ApplicationId}", id);
                throw new NotFoundException("Application", id.ToString());
            }
            
            var result = await _unitOfWork.Applications.DeleteAsync(id);
            
            await _unitOfWork.CommitAsync();
            
            _logger.LogInformation("Successfully deleted application: {ApplicationId}", id);
            
            // Send denial email (after successful commit)
            try
            {
                await _smtpService.SendEmailWithCodeAsync(
                    application.FirstName,
                    application.LastName,
                    application.Email,
                    application.Code,
                    EmailTemplateType.ApplicationDenied
                );
                
                _logger.LogInformation("Deletion email sent for application: {ApplicationId}", id);
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, "Failed to send deletion email for application: {ApplicationId}", id);
                // Don't throw here - the deletion was successful
            }
            
            return result;
        }
        catch (ApplicationException)
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Unexpected error deleting application: {ApplicationId}", id);
            throw;
        }
    }
}