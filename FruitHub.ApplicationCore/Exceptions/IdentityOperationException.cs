using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class IdentityOperationException : AppException
{
    public IReadOnlyList<string> Errors { get; }

    public IdentityOperationException(IEnumerable<string> errors)
        : base(
            message: "Identity operation failed.",
            errorCode: ErrorsCode.IdentityOperationFailed,
            statusCode: 400)
    {
        Errors = errors.ToList();
    }
}