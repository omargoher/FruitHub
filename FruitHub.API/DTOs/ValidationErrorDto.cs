namespace FruitHub.API.DTOs;

public class ValidationErrorDto
{
    public string Field { get; set; } = null!;
    public string Error { get; set; } = null!;
}