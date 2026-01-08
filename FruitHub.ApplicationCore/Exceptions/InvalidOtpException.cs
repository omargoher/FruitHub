using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidOtpException : AppException
{
    public InvalidOtpException() : base(
        message: "Invalid or expired OTP.",
        errorCode: ErrorsCode.InvalidOtp,
        statusCode: 400)
    {}
}