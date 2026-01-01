namespace FruitHub.ApplicationCore.Exceptions;

public class EmailNotConfirmedException : AppException
{
    public EmailNotConfirmedException() : 
        base("Email is not Confirmed", 403){}
}