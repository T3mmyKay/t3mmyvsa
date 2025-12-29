using System.Text.Json.Serialization;

namespace T3mmyvsa.Models.Shared;

public class PaginatedResponse<T>(List<T> data, PaginationMeta meta, PaginationLinks links)
{
    [JsonPropertyName("data")]
    public List<T> Data { get; } = data;

    [JsonPropertyName("meta")]
    public PaginationMeta Meta { get; } = meta;

    [JsonPropertyName("links")]
    public PaginationLinks Links { get; } = links;
}

public class PaginationMeta
{
    [JsonPropertyName("current_page")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("from")]
    public int? From { get; set; }

    [JsonPropertyName("last_page")]
    public int LastPage { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("per_page")]
    public int PerPage { get; set; }

    [JsonPropertyName("to")]
    public int? To { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class PaginationLinks
{
    [JsonPropertyName("first")]
    public string First { get; set; } = string.Empty;

    [JsonPropertyName("last")]
    public string Last { get; set; } = string.Empty;

    [JsonPropertyName("prev")]
    public string? Prev { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}
