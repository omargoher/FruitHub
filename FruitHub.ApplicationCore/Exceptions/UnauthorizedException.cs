using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class UnauthorizedException : AppException
{
    public UnauthorizedException()
        : base(
            message: "You are not authenticated.",
            errorCode: ErrorsCode.Unauthorized,
            statusCode: 401)
    {
    }
}