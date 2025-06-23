namespace UniTrackRemaster.Api.Dto.User;

public class UserPaginationParams
{
    private int _pageNumber = 1;
    private int _pageSize = 20;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 20 : value > 100 ? 100 : value;
    }

    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool Ascending { get; set; } = true;
    public string? Role { get; set; } 
}