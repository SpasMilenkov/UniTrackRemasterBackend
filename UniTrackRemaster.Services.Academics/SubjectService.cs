using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class SubjectService : ISubjectService
{
    private readonly ISubjectRepository _subjectRepository;
    private readonly ILogger<SubjectService> _logger;

    public SubjectService(
        ISubjectRepository subjectRepository,
        ILogger<SubjectService> logger)
    {
        _subjectRepository = subjectRepository;
        _logger = logger;
    }

    public async Task<SubjectResponseDto> GetByIdAsync(Guid id)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new NotFoundException("Subject not found");
        return SubjectResponseDto.FromEntity(subject);
    }

    public async Task<IEnumerable<SubjectResponseDto>> GetAllAsync()
    {
        var subjects = await _subjectRepository.GetAllAsync();
        return subjects.Select(SubjectResponseDto.FromEntity);
    }

    public async Task<SubjectResponseDto> CreateAsync(CreateSubjectDto dto)
    {
        var subject = new Subject()
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            ShortDescription = dto.ShortDescription,
            DetailedDescription = dto.DetailedDescription,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _subjectRepository.CreateAsync(subject);
        return SubjectResponseDto.FromEntity(subject);
    }

    public async Task<SubjectResponseDto> UpdateAsync(Guid id, UpdateSubjectDto dto)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new NotFoundException("Subject not found");

        if (dto.Name != null) subject.Name = dto.Name;
        if (dto.ShortDescription != null) subject.ShortDescription = dto.ShortDescription;
        if (dto.DetailedDescription != null) subject.DetailedDescription = dto.DetailedDescription;
        subject.UpdatedAt = DateTime.UtcNow;

        await _subjectRepository.UpdateAsync(subject);
        return SubjectResponseDto.FromEntity(subject);
    }

    public async Task DeleteAsync(Guid id)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new NotFoundException("Subject not found");

        await _subjectRepository.DeleteAsync(id);
    }
}