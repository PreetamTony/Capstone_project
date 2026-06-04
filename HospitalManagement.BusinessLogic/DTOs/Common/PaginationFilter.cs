namespace HospitalManagement.BusinessLogic.DTOs.Common;

/// <summary>Pagination and search filter passed to all list endpoints.</summary>
public class PaginationFilter
{
    private int _pageSize = 10;
    private int _pageNumber = 1;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value < 1 ? 1 : value;
    }

    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
