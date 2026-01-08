using FruitHub.ApplicationCore.Errors;

namespace FruitHub.ApplicationCore.Exceptions;

public class EmailNotConfirmedException : AppException
{
    public EmailNotConfirmedException() :
        base(message: "Email address is not confirmed.",
            errorCode: ErrorsCode.EmailNotConfirmed,
            statusCode: 403)
    {
        
    }
}