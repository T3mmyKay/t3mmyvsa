using T3mmyvsa.Models.Shared;

namespace T3mmyvsa.Extensions;

public static class PaginationExtensions
{
    public static PaginatedResponse<TDestination> ToPaginatedResponse<TSource, TDestination>(
        this PagedList<TSource> pagedList,
        Func<List<TSource>, List<TDestination>> mapper,
        string path)
    {
        var data = mapper(pagedList);
        
        var meta = new PaginationMeta
        {
            CurrentPage = pagedList.CurrentPage,
            From = pagedList.TotalCount == 0 ? null : (pagedList.CurrentPage - 1) * pagedList.PageSize + 1,
            To = pagedList.TotalCount == 0 ? null : Math.Min(pagedList.CurrentPage * pagedList.PageSize, pagedList.TotalCount),
            LastPage = pagedList.TotalPages,
            Path = path,
            PerPage = pagedList.PageSize,
            Total = pagedList.TotalCount
        };

        var links = new PaginationLinks
        {
            First = $"{path}?page=1&per_page={pagedList.PageSize}",
            Last = $"{path}?page={pagedList.TotalPages}&per_page={pagedList.PageSize}",
            Prev = pagedList.HasPrevious ? $"{path}?page={pagedList.CurrentPage - 1}&per_page={pagedList.PageSize}" : null,
            Next = pagedList.HasNext ? $"{path}?page={pagedList.CurrentPage + 1}&per_page={pagedList.PageSize}" : null
        };

        return new PaginatedResponse<TDestination>(data, meta, links);
    }
}
