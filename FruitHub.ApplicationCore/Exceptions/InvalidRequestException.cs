using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class InvalidRequestException : AppException
{
    public InvalidRequestException(string message) 
        : base(
            message: message,
            errorCode: ErrorsCode.InvalidRequest,
            statusCode: 400)
    {}
}