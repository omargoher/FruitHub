namespace FruitHub.ApplicationCore.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string msg) : base(msg, 404)
    {}
}