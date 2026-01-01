namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidCredentialsException : AppException 
{
    public InvalidCredentialsException() : 
        base("Email or Password incorrect", 401)
    { }
}