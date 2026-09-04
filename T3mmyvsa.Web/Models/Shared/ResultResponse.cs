namespace T3mmyvsa.Models.Shared;

public sealed class ResultResponse
{
    public Guid? Id { get; init; }
    public required string Message { get; init; }
    public bool Success { get; init; } = true;
}
