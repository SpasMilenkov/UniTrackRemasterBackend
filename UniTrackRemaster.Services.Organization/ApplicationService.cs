using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Organizations;
using UniTrackRemaster.Services.Messaging;
using UniTrackRemaster.Services.Messaging.Enums;

namespace UniTrackRemaster.Services.Organization;
public class ApplicationService(IUnitOfWork unitOfWork, ISmtpService smtpService) : IApplicationService
{
    public async Task<ApplicationResponseDto> GetByIdAsync(Guid id)
    {
        var application = await unitOfWork.Applications.GetByIdAsync(id)
            ?? throw new NotFoundException($"Application with ID {id} not found");
            
        if(application.Institution is null) 
            throw new NotFoundException($"Institution not found for application {id}");
        
        return ApplicationResponseDto.FromEntity(
            application, 
            AddressDto.FromEntity(application.Institution.Address));
    }

    public async Task<ApplicationResponseDto> GetByCodeAsync(string code, string email)
    {
        var application = await unitOfWork.Applications.GetByEmailAsync(email)
            ?? throw new NotFoundException($"Application not found for email {email}");
            
        if(application.Institution is null) 
            throw new NotFoundException($"Institution not found for application {application.Id}");
            
        if(application.Code != code) 
            throw new InvalidOperationException($"Invalid application code: {code}");

        return ApplicationResponseDto.FromEntity(
            application, 
            AddressDto.FromEntity(application.Institution.Address));
    }

    public async Task<ApplicationResponseDto> GetByInstitutionIdAsync(Guid id)
    {
        var application = await unitOfWork.Applications.GetByInstitutionIdAsync(id)
                          ?? throw new NotFoundException($"Application not found for institution {id}");

        if (application.Institution is null)
            throw new NotFoundException($"Institution not found for application {application.Id}");

        return ApplicationResponseDto.FromEntity(
            application,
            AddressDto.FromEntity(application.Institution.Address));
    }
    
    public async Task<List<ApplicationResponseDto>> GetAllAsync()
    {
        var applications = await unitOfWork.Applications.GetAllAsync();
        
        return applications
            .Where(a => a.Institution != null)
            .Select(a => ApplicationResponseDto.FromEntity(
                a, 
                AddressDto.FromEntity(a.Institution.Address)))
            .ToList();
    }

    public async Task ApproveAsync(Guid id)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var application = await unitOfWork.Applications.ApproveAsync(id)
                              ?? throw new NotFoundException($"Application with ID {id} not found");
            
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();
                
            await smtpService.SendEmailWithCodeAsync(
                application.FirstName,
                application.LastName,
                application.Email,
                application.Code,
                EmailTemplateType.ApplicationApproved
            );
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApplicationResponseDto> CreateAsync(CreateInstitutionApplicationDto application)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var existingApplication = await unitOfWork.Applications.GetByEmailAsync(application.Email);
            if (existingApplication is not null) 
                throw new InvalidOperationException($"Application already exists for email {application.Email}");

            var institution = await unitOfWork.Institutions.AddAsync(new Institution()
            {
                Name = application.InstitutionName,
                Type = application.InstitutionType,
                Address = AddressDto.ToEntity(application.Address) ,
                Email = application.Email,
                Phone = application.Phone
            });
        
            var applicationEntity = await unitOfWork.Applications.CreateAsync(
                CreateInstitutionApplicationDto.ToEntity(application, institution.Id));
            
            if (applicationEntity is null)
                throw new InvalidOperationException("Failed to create application");
            
            if (applicationEntity.Institution is null) 
                throw new NotFoundException($"Institution not found for application {applicationEntity.Id}");

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();
        
            await smtpService.SendEmailWithCodeAsync(
                application.FirstName,
                application.LastName,
                application.Email,
                applicationEntity.Code,
                EmailTemplateType.ApplicationCreated
            );
        
            return ApplicationResponseDto.FromEntity(
                applicationEntity, 
                AddressDto.FromEntity(applicationEntity.Institution.Address));
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<ApplicationResponseDto> UpdateAsync(Guid id, UpdateInstitutionApplicationDto updatedApplication)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var application = await unitOfWork.Applications.UpdateAsync(id,
                                  UpdateInstitutionApplicationDto.ToEntity(updatedApplication))
                              ?? throw new NotFoundException($"Application with ID {id} not found");
        
            if(application.Institution is null) 
                throw new NotFoundException($"Institution not found for application {id}");

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();
        
            return ApplicationResponseDto.FromEntity(
                application, 
                AddressDto.FromEntity(application.Institution.Address));
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var application = await unitOfWork.Applications.GetByIdAsync(id)
                ?? throw new NotFoundException($"Application with ID {id} not found");
            
            var result = await unitOfWork.Applications.DeleteAsync(id);
            
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();
            
            await smtpService.SendEmailWithCodeAsync(
                application.FirstName,
                application.LastName,
                application.Email,
                application.Code,
                EmailTemplateType.ApplicationDenied
            );
            
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}