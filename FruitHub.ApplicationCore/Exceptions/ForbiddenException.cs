namespace FruitHub.ApplicationCore.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException()
        : base("Access to this resource is forbidden.", 403)
    {
    }

    public ForbiddenException(string message)
        : base(message, 403)
    {
    }
}