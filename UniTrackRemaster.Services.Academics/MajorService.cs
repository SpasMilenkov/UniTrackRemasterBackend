using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Major;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class MajorService : IMajorService
{
    private readonly IMajorRepository _majorRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger<MajorService> _logger;

    public MajorService(
        IMajorRepository majorRepository,
        IFacultyRepository facultyRepository,
        ILogger<MajorService> logger)
    {
        _majorRepository = majorRepository;
        _facultyRepository = facultyRepository;
        _logger = logger;
    }

    public async Task<MajorResponseDto> CreateAsync(CreateMajorDto dto)
    {
        var faculty = await _facultyRepository.GetByIdAsync(dto.FacultyId);
        if (faculty == null)
            throw new NotFoundException("Faculty not found");

        var major = new Major
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            ShortDescription = dto.ShortDescription,
            DetailedDescription = dto.DetailedDescription,
            RequiredCredits = dto.RequiredCredits,
            DurationInYears = dto.DurationInYears,
            CareerOpportunities = dto.CareerOpportunities,
            AdmissionRequirements = dto.AdmissionRequirements,
            FacultyId = dto.FacultyId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _majorRepository.CreateAsync(major);
        return MajorResponseDto.FromEntity(major);
    }

    public async Task<MajorResponseDto> UpdateAsync(Guid id, UpdateMajorDto dto)
    {
        var major = await _majorRepository.GetByIdAsync(id);
        if (major == null)
            throw new NotFoundException("Major not found");

        if (dto.Name != null) major.Name = dto.Name;
        if (dto.Code != null) major.Code = dto.Code;
        if (dto.ShortDescription != null) major.ShortDescription = dto.ShortDescription;
        if (dto.DetailedDescription != null) major.DetailedDescription = dto.DetailedDescription;
        if (dto.RequiredCredits.HasValue) major.RequiredCredits = dto.RequiredCredits.Value;
        if (dto.DurationInYears.HasValue) major.DurationInYears = dto.DurationInYears.Value;
        if (dto.CareerOpportunities != null) major.CareerOpportunities = dto.CareerOpportunities;
        if (dto.AdmissionRequirements != null) major.AdmissionRequirements = dto.AdmissionRequirements;

        major.UpdatedAt = DateTime.UtcNow;
        await _majorRepository.UpdateAsync(major);
        return MajorResponseDto.FromEntity(major);
    }

    public async Task<MajorResponseDto> GetByIdAsync(Guid id)
    {
        var major = await _majorRepository.GetByIdAsync(id);
        if (major == null)
            throw new NotFoundException("Major not found");
        return MajorResponseDto.FromEntity(major);
    }

    public async Task<IEnumerable<MajorResponseDto>> GetByFacultyAsync(Guid facultyId)
    {
        var majors = await _majorRepository.GetByFacultyAsync(facultyId);
        return majors.Select(MajorResponseDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var major = await _majorRepository.GetByIdAsync(id);
        if (major == null)
            throw new NotFoundException("Major not found");

        if (major.Students?.Any() == true)
            throw new ValidationException("Cannot delete major with enrolled students");

        if (major.Courses?.Any() == true)
            throw new ValidationException("Cannot delete major with associated courses");

        await _majorRepository.DeleteAsync(id);
    }
}