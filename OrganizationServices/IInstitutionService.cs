using Microsoft.AspNetCore.Http;
using UniTrackRemaster.Api.Dto.Institution;
using UniTrackRemaster.Api.Dto.Request;

namespace OrganizationServices;

public interface IInstitutionService
{
    Task<InstitutionDto> GetByIdAsync(Guid id);
    Task<List<InstitutionDto>> GetAllAsync();
   Task UpdateAsync(Guid id, UpdateInstitutionDto updateDto, IFormFile? logo = null,
       IEnumerable<IFormFile>? newImages = null);
    Task DeleteAsync(Guid id);
    Task<List<InstitutionDto>> GetInstitutionsByUserId(string userId);
}