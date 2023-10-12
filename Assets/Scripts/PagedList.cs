using System.Collections.Generic;
using System.Linq;

public class PagedList<T>
{
    private PagedList(List<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public List<T> Items { get; }
    public int Page { get; }
    private int PageSize { get; }
    public int TotalCount { get; }

    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;

    public static PagedList<T> Create(IEnumerable<T> query, int page, int pageSize)
    {
        var totalCount = query.Count();

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new(items, page, pageSize, totalCount);
    }
}
