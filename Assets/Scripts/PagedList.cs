using System.Collections.Generic;
using System.Linq;

public class PagedList<T>
{
    private PagedList(List<T> items, int page, int pageSize, int totalCount)
    {
        this.items = items;
        this.page = page;
        this.pageSize = pageSize;
        this.totalCount = totalCount;
    }

    public List<T> items { get; }
    public int page { get; }
    private int pageSize { get; }
    public int totalCount { get; }

    public bool hasNextPage => page * pageSize < totalCount;
    public bool hasPreviousPage => page > 1;

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
