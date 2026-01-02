namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidOtp : AppException
{
    public InvalidOtp(string msg) : base(msg, 400)
    {}
}