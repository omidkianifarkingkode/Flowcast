using System.Text.Json.Serialization;

namespace SharedKernel.Pagination;

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("page_size")]
    public int PageSize { get; set; }

    [JsonPropertyName("total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("has_previous")]
    public bool HasPrevious => CurrentPage > 1;

    [JsonPropertyName("has_next")]
    public bool HasNext => CurrentPage < TotalPages;

    public PagedResult()
    {
        Items = new List<T>();
    }

    public PagedResult(List<T>? items, int totalCount, int currentPage, int pageSize)
    {
        Items = items ?? [];
        TotalCount = Math.Max(0, totalCount);
        CurrentPage = Math.Max(1, currentPage);
        PageSize = Math.Max(1, pageSize);
        TotalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }
}
