namespace UniTrackRemaster.Api.Dto.Teacher.Analytics;


/// <summary>
/// At-risk students response with semester context
/// </summary>
public class AtRiskStudentsResponseDto
{
    /// <summary>Threshold percentage used for high risk classification</summary>
    public int HighRiskThreshold { get; set; }

    /// <summary>Threshold percentage used for medium risk classification</summary>
    public int MediumRiskThreshold { get; set; }

    /// <summary>Total class days used for calculating absence rates</summary>
    public int TotalClassDays { get; set; }

    /// <summary>List of students identified as at-risk</summary>
    public List<AtRiskStudentDto> AtRiskStudents { get; set; } = new();

    /// <summary>ID of the semester this analysis represents</summary>
    public Guid? SemesterId { get; set; }

    /// <summary>Name of the semester this analysis represents</summary>
    public string? SemesterName { get; set; }
}