namespace FruitHub.API.DTOs;

public class ErrorResponse
{
    public string Code { get; set; } = null!;
    public string Message { get; init; } = null!;
    public object? Errors { get; init; }
}