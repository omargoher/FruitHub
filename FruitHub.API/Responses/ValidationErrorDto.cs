namespace FruitHub.API.Responses;

public class ValidationError
{
    public string Field { get; set; } = null!;
    public string Error { get; set; } = null!;
}