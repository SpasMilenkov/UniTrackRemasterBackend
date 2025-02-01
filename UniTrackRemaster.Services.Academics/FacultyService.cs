using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Faculty;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class FacultyService : IFacultyService
{
    private readonly IFacultyRepository _facultyRepository;
    private readonly IInstitutionRepository _universityRepository;
    private readonly ILogger<FacultyService> _logger;

    public FacultyService(
        IFacultyRepository facultyRepository,
        IInstitutionRepository universityRepository,
        ILogger<FacultyService> logger)
    {
        _facultyRepository = facultyRepository;
        _universityRepository = universityRepository;
        _logger = logger;
    }

    public async Task<FacultyResponseDto> CreateAsync(CreateFacultyDto dto)
    {
        var university = await _universityRepository.GetByIdAsync(dto.UniversityId);
        if (university == null)
            throw new NotFoundException("University not found");

        var faculty = new Faculty
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            ShortDescription = dto.ShortDescription,
            DetailedDescription = dto.DetailedDescription,
            Website = dto.Website,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            Status = dto.Status,
            UniversityId = dto.UniversityId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _facultyRepository.CreateAsync(faculty);
        return FacultyResponseDto.FromEntity(faculty);
    }

    public async Task<FacultyResponseDto> UpdateAsync(Guid id, UpdateFacultyDto dto)
    {
        var faculty = await _facultyRepository.GetByIdAsync(id);
        if (faculty == null)
            throw new NotFoundException("Faculty not found");

        if (dto.Name != null) faculty.Name = dto.Name;
        if (dto.Code != null) faculty.Code = dto.Code;
        if (dto.ShortDescription != null) faculty.ShortDescription = dto.ShortDescription;
        if (dto.DetailedDescription != null) faculty.DetailedDescription = dto.DetailedDescription;
        if (dto.Website != null) faculty.Website = dto.Website;
        if (dto.ContactEmail != null) faculty.ContactEmail = dto.ContactEmail;
        if (dto.ContactPhone != null) faculty.ContactPhone = dto.ContactPhone;
        if (dto.Status.HasValue) faculty.Status = dto.Status.Value;

        faculty.UpdatedAt = DateTime.UtcNow;
        await _facultyRepository.UpdateAsync(faculty);
        return FacultyResponseDto.FromEntity(faculty);
    }

    public async Task<FacultyResponseDto> GetByIdAsync(Guid id)
    {
        var faculty = await _facultyRepository.GetByIdAsync(id);
        if (faculty == null)
            throw new NotFoundException("Faculty not found");
        return FacultyResponseDto.FromEntity(faculty);
    }

    public async Task<IEnumerable<FacultyResponseDto>> GetByUniversityAsync(Guid universityId)
    {
        var faculties = await _facultyRepository.GetByUniversityAsync(universityId);
        return faculties.Select(FacultyResponseDto.FromEntity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var faculty = await _facultyRepository.GetByIdAsync(id);
        if (faculty == null)
            throw new NotFoundException("Faculty not found");

        if (faculty.Majors?.Any() == true)
            throw new ValidationException("Cannot delete faculty with associated majors");

        if (faculty.Departments?.Any() == true)
            throw new ValidationException("Cannot delete faculty with associated departments");

        await _facultyRepository.DeleteAsync(id);
    }
}
