namespace T3mmyvsa.Models.Shared;

public class PagedList<T>(List<T> items, int count, int pageNumber, int pageSize) : List<T>(items)
{
    public int CurrentPage { get; } = pageNumber;
    public int TotalPages { get; } = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
    public int PageSize { get; } = pageSize;
    public int TotalCount { get; } = count;

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
        }

        if (pageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be between 1 and 100.");
        }

        var count = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
