namespace MainService.Domain.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }

    public PagedResult()
    {
    }

    public PagedResult(List<T> items, int totalCount)
    {
        Items = items;
        TotalCount = totalCount;
    }
}


