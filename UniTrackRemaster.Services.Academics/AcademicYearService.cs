using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.AcademicYear;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class AcademicYearService : IAcademicYearService
{
    private readonly IAcademicYearRepository _academicYearRepository;
    private readonly IInstitutionRepository _institutionRepository;

    public AcademicYearService(
        IAcademicYearRepository academicYearRepository,
        IInstitutionRepository institutionRepository)
    {
        _academicYearRepository = academicYearRepository;
        _institutionRepository = institutionRepository;
    }
    public async Task<AcademicYearResponseDto> GetByIdAsync(Guid id)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(id);
        if (academicYear == null)
            throw new NotFoundException("Academic year not found");
        return AcademicYearResponseDto.FromEntity(academicYear);
    }

    public async Task<IEnumerable<AcademicYearResponseDto>> GetByInstitutionAsync(Guid institutionId)
    {
        var academicYears = await _academicYearRepository.GetByInstitutionAsync(institutionId);
        return academicYears.Select(AcademicYearResponseDto.FromEntity);
    }
    
    public async Task<AcademicYearResponseDto> CreateAsync(CreateAcademicYearDto dto)
    {
        var institution = await _institutionRepository.GetByIdAsync(dto.InstitutionId);
        if (institution == null)
            throw new ValidationException("Educational institution not found");

        if (dto.StartDate >= dto.EndDate)
            throw new ValidationException("Start date must be before end date");

        var academicYear = new AcademicYear
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            InstitutionId = dto.InstitutionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _academicYearRepository.CreateAsync(academicYear);
        return AcademicYearResponseDto.FromEntity(academicYear);
    }

    public async Task<AcademicYearResponseDto> UpdateAsync(Guid id, UpdateAcademicYearDto dto)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(id);
        if (academicYear == null) 
            throw new NotFoundException("Academic year not found");

        if (dto.Name != null)
            academicYear.Name = dto.Name;
        
        if (dto.StartDate.HasValue && dto.EndDate.HasValue)
        {
            if (dto.StartDate >= dto.EndDate)
                throw new ValidationException("Start date must be before end date");
                
            academicYear.StartDate = dto.StartDate.Value;
            academicYear.EndDate = dto.EndDate.Value;
        }
        else if (dto.StartDate.HasValue || dto.EndDate.HasValue)
        {
            var newStartDate = dto.StartDate ?? academicYear.StartDate;
            var newEndDate = dto.EndDate ?? academicYear.EndDate;
            
            if (newStartDate >= newEndDate)
                throw new ValidationException("Start date must be before end date");
                
            academicYear.StartDate = newStartDate;
            academicYear.EndDate = newEndDate;
        }

        academicYear.UpdatedAt = DateTime.UtcNow;
        await _academicYearRepository.UpdateAsync(academicYear);
        return AcademicYearResponseDto.FromEntity(academicYear);
    }

    public async Task DeleteAsync(Guid id)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(id);
        if (academicYear == null)
            throw new NotFoundException("Academic year not found");

        if (academicYear.Semesters?.Any() == true)
            throw new ValidationException("Cannot delete academic year with associated semesters");

        await _academicYearRepository.DeleteAsync(id);
    }
}