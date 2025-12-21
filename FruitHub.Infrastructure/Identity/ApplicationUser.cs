using Microsoft.AspNetCore.Identity;

namespace FruitHub.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}