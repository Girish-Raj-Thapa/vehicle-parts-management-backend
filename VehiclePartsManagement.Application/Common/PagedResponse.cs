namespace VehiclePartsManagement.Application.Common;

public class PagedResponse<T> : ApiResponse<T>
{
    public PaginationMeta Pagination { get; set; } = new();

    public static PagedResponse<T> SuccessPagedResponse(
        T data,
        PaginationMeta pagination,
        string message = "Request completed successfully")
    {
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Pagination = pagination
        };
    }
}

public class PaginationMeta
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}
