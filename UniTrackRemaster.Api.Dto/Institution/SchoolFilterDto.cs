namespace UniTrackRemaster.Api.Dto.Request;

public class SchoolFilterDto
{
    public string? SearchTerm { get; set; }
    public List<string>? Types { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
