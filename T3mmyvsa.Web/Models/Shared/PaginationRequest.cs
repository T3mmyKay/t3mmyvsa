using System.ComponentModel;

namespace T3mmyvsa.Models.Shared;

public class PaginationRequest
{
    [FromQuery(Name = "page")]
    [DefaultValue(1)]
    public int? Page { get; set; } = 1;

    [FromQuery(Name = "per_page")]
    [DefaultValue(15)]
    public int? PageSize { get; set; } = 15;
}
