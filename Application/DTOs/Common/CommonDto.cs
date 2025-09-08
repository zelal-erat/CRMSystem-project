namespace CRMSystem.Application.DTOs.Common;

public class PaginatedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }

    // Static factory method - daha kısa yazım için
    public static PaginatedResultDto<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PaginatedResultDto<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            HasPreviousPage = pageNumber > 1,
            HasNextPage = pageNumber * pageSize < totalCount
        };
    }
}

