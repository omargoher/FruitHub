using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string resource)
        : base(
            message: $"{resource} was not found.",
            errorCode: ErrorsCode.NotFound,
            statusCode: 404)
    {
    }
}