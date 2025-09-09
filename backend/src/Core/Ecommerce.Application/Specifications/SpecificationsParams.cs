namespace Ecommerce.Application.Specifications;

public abstract class SpecificationsParams
{
    public string? Sort { get; set; }
    public int PageIndex { get; set; } = 1;
    public const int MaxPageSize = 50;
    public int _pageSize = 3;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}