using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.Infrastructure.DTOs;
using FruitHub.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;

namespace FruitHub.Infrastructure.Interfaces;

public interface ITokenService
{
    Task<SecurityToken> GenerateJwtAsync(ApplicationUser user);
    Task<RefreshTokenDto> CreateRefreshTokenAsync(ApplicationUser user, string? refreshToken = null);
    void RevokeAllAsync(ApplicationUser user);
}