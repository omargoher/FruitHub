namespace FruitHub.ApplicationCore.Exceptions;

public class IdentityOperationException : AppException
{
    public IReadOnlyList<string> Errors { get; }

    public IdentityOperationException(IEnumerable<string> errors)
        : base("Identity operation failed", 400)
    {
        Errors = errors.ToList();
    }
}