namespace FruitHub.ApplicationCore.Exceptions;

public class RegistrationFailedException : AppException
{
    public RegistrationFailedException(string message)
        : base(message, 500) { }
}