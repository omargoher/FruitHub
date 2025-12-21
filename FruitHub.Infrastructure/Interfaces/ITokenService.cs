using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;

namespace FruitHub.Infrastructure.Interfaces;

public interface ITokenService
{
    Task<SecurityToken> GenerateJwtAsync(ApplicationUser user);
    Task<RefreshTokenDto> CreateRefreshTokenAsync(ApplicationUser user);
    Task RevokeAllAsync(ApplicationUser user);
}