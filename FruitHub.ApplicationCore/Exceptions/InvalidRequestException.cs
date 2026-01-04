namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidRequestException : AppException
{
    public InvalidRequestException(string msg) : base(msg, 400)
    {}
}