using UniTrackRemaster.Data.Models.Academical;
using UniTrackRemaster.Data.Models.Enums;

namespace UniTrackRemaster.Services.Organization.Strategies;

/// <summary>
/// Interface for grading system strategies
/// </summary>
public interface IGradingStrategy
{
    string ConvertScoreToGrade(decimal score);
    double ConvertScoreToGpaPoints(decimal score);
    string DetermineStatus(decimal score, decimal passingScore);
    decimal ConvertGradeToScore(string grade);
    GradingSystemType GetSystemType();
    GradingSystem CreateDefaultGradingSystem(Guid institutionId);
}
