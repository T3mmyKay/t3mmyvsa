using System.ComponentModel;

namespace T3mmyvsa.Models.Shared;

public class PaginationRequest
{
    // Auto-properties used for validation

    [FromQuery(Name = "page")]
    [DefaultValue(1)]
    [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
    public int? Page { get; set; } = 1;

    [FromQuery(Name = "per_page")]
    [DefaultValue(15)]
    [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
    public int? PageSize { get; set; } = 15;
}
