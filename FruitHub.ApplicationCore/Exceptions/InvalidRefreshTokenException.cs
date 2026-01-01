namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidRefreshTokenException: AppException
{
    public InvalidRefreshTokenException()
        : base("Invalid refresh token", 401)
    {
    }
}