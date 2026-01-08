using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException()
        : base(
            message: "You do not have permission to access this resource.",
            errorCode: ErrorsCode.Forbidden,
            statusCode: 403)
    {
    }
}