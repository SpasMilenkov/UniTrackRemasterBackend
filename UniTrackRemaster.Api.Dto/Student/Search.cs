namespace UniTrackRemaster.Api.Dto.Student;

// Create this in your DTOs folder
public class StudentSearchRequestDto
{
    public string Query { get; set; }
    public Guid? GradeId { get; set; }
    public Guid? InstitutionId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "FirstName";
    public bool Ascending { get; set; } = true;
}

public class PaginatedStudentResponseDto
{
    public IEnumerable<StudentResponseDto> Students { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    public static PaginatedStudentResponseDto Create(
        IEnumerable<StudentResponseDto> students,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PaginatedStudentResponseDto
        {
            Students = students,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber < totalPages
        };
    }
}