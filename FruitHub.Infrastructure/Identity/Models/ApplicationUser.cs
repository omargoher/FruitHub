using Microsoft.AspNetCore.Identity;

namespace FruitHub.Infrastructure.Identity.Models;

public class ApplicationUser : IdentityUser
{
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}