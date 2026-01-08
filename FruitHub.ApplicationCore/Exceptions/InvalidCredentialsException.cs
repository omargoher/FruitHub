using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidCredentialsException : AppException 
{
    public InvalidCredentialsException() : 
        base(
            message: "Invalid email or password.",
            errorCode: ErrorsCode.InvalidCredentials,
            statusCode: 401)
    {
    }
}