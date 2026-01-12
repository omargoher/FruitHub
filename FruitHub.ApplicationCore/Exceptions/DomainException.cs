using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class DomainException : AppException
{
    public DomainException(string message) 
        : base(
            message: message,
            errorCode: ErrorsCode.DomainRuleViolation,
            statusCode: 400)
    {}
}