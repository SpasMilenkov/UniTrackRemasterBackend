namespace UniTrackRemaster.Api.Dto.Metrics;

public record AcademicStatisticsDto(
    int TotalInstitutions,
    int SchoolCount,
    int UniversityCount,
    int FacultyCount,
    int DepartmentCount,
    int MajorCount,
    int SubjectCount,
    int GradeCount,
    Dictionary<string, int> InstitutionsByType
);