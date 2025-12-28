namespace FruitHub.ApplicationCore.DTOs.User;

public class UserProfileDto
{
    public int Id {get; set;}
    public string FullName {get; set;} = null!;
    public string Email {get; set;} = null!;
}