using System.ComponentModel.DataAnnotations;

namespace UniTrackRemaster.Api.Dto.Teacher;


public class TeacherSearchParams
{
    /// <summary>
    /// General search query (searches in name, email, title)
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Filter by institution ID
    /// </summary>
    public Guid? InstitutionId { get; set; }

    /// <summary>
    /// Filter by department ID
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Filter by class grade ID (homeroom teacher)
    /// </summary>
    public Guid? ClassGradeId { get; set; }

    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination
    /// </summary>
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Field to sort by (e.g., "firstName", "lastName", "email", "title")
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (true for ascending, false for descending)
    /// </summary>
    public bool Ascending { get; set; } = true;
}

public class TeacherSearchResponse
{
    public IEnumerable<TeacherResponseDto> Teachers { get; set; } = new List<TeacherResponseDto>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
}