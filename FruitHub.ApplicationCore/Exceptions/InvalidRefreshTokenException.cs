using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidRefreshTokenException: AppException
{
    public InvalidRefreshTokenException()
        : base(message: "Invalid or expired refresh token.",
            errorCode: ErrorsCode.InvalidRefreshToken,
            statusCode: 401)
    {}
}