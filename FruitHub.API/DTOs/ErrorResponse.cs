namespace FruitHub.API.DTOs;

public class ErrorResponse
{
    public string Message { get; init; } = null!;
    public IReadOnlyList<string>? Errors { get; init; }
}