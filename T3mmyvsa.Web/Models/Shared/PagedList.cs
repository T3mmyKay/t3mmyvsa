namespace T3mmyvsa.Models.Shared;

public class PagedList<T>(List<T> items, int count, int pageNumber, int pageSize) : List<T>(items)
{
    public int CurrentPage { get; } = pageNumber;
    public int TotalPages { get; } = (int)Math.Ceiling(count / (double)pageSize);
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = count;

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
