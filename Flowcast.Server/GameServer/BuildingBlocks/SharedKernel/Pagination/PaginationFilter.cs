namespace SharedKernel.Pagination;

public class PaginationFilter
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    public string? SortBy { get; set; } = string.Empty;
    public bool IsDescending { get; set; } = false;
    public string? SearchTerm { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
