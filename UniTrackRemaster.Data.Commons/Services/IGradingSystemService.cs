using UniTrackRemaster.Api.Dto.Grading;

namespace UniTrackRemaster.Commons.Services;

    public interface IGradingSystemService
    {
        Task<GradingSystemResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<GradingSystemResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<GradingSystemResponseDto>> GetAllForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
        Task<GradingSystemResponseDto> GetDefaultForInstitutionAsync(Guid institutionId, CancellationToken cancellationToken = default);
        Task<GradingSystemResponseDto> CreateAsync(CreateGradingSystemDto dto, CancellationToken cancellationToken = default);
        Task<GradingSystemResponseDto> UpdateAsync(Guid id, UpdateGradingSystemDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> SetDefaultAsync(Guid id, Guid institutionId, CancellationToken cancellationToken = default);
        Task<bool> InitializeDefaultGradingSystemsAsync(Guid institutionId, CancellationToken cancellationToken = default);
        Task<bool> InitializeDefaultGradingSystemsWithinTransactionAsync(Guid institutionId, CancellationToken cancellationToken = default);
        // Grade conversion methods
        string ConvertScoreToGrade(decimal score, Guid gradingSystemId);
        double ConvertScoreToGpaPoints(decimal score, Guid gradingSystemId);
        string DetermineStatus(decimal score, Guid gradingSystemId);
        decimal ConvertGradeToScore(string grade, Guid gradingSystemId);
    }
    