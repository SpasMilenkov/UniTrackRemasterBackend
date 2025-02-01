using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using UniTrackRemaster.Api.Dto.Grade;
using UniTrackRemaster.Api.Dto.Request;
using UniTrackRemaster.Api.Dto.Response;
using UniTrackRemaster.Commons;
using UniTrackRemaster.Data.Exceptions;
using UniTrackRemaster.Data.Models.Academical;

namespace UniTrackRemaster.Services.Academics;

public class GradeService : IGradeService
{
    private readonly IGradeRepository _gradeRepository;
    private readonly ILogger<GradeService> _logger;

    public GradeService(
        IGradeRepository gradeRepository,
        ILogger<GradeService> logger)
    {
        _gradeRepository = gradeRepository;
        _logger = logger;
    }

    public async Task<GradeResponseDto> GetByIdAsync(Guid id)
    {
        var grade = await _gradeRepository.GetByIdAsync(id);
        if (grade == null) throw new NotFoundException("Grade not found");
        return GradeResponseDto.FromEntity(grade);
    }

    public async Task<IEnumerable<GradeResponseDto>> GetAllAsync()
    {
        var grades = await _gradeRepository.GetAllAsync();
        return grades.Select(GradeResponseDto.FromEntity);
    }

    public async Task<GradeResponseDto> CreateAsync(CreateGradeDto dto)
    {
        var grade = new Grade
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Students = null
        };

        await _gradeRepository.CreateAsync(grade);
        return GradeResponseDto.FromEntity(grade);
    }

    public async Task<GradeResponseDto> UpdateAsync(Guid id, UpdateGradeDto dto)
    {
        var grade = await _gradeRepository.GetByIdAsync(id);
        if (grade == null) throw new NotFoundException("Grade not found");

        grade.Name = dto.Name;
        grade.UpdatedAt = DateTime.UtcNow;

        await _gradeRepository.UpdateAsync(grade);
        return GradeResponseDto.FromEntity(grade);
    }

    public async Task DeleteAsync(Guid id)
    {
        var grade = await _gradeRepository.GetByIdAsync(id);
        if (grade == null) throw new NotFoundException("Grade not found");

        if (grade.Students?.Any() == true ||
            grade.ElectiveSubjects?.Any() == true ||
            grade.HomeRoomTeacher != null 
            )
        {
            throw new ValidationException("Cannot delete grade with associated records");
        }

        await _gradeRepository.DeleteAsync(id);
    }
}