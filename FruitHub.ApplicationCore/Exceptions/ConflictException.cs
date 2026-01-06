namespace FruitHub.ApplicationCore.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409)
    {
    }
}