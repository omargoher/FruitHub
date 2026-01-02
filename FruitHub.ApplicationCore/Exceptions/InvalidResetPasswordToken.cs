namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidResetPasswordToken : AppException
{
    public InvalidResetPasswordToken() : base("Invalid Token", 400)
    {}
}