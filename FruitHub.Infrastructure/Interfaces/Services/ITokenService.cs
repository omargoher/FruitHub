using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.Infrastructure.Models;
using FruitHub.Infrastructure.Identity.Models;
using Microsoft.IdentityModel.Tokens;

namespace FruitHub.Infrastructure.Interfaces.Services;

public interface ITokenService
{
    Task<SecurityToken> GenerateJwtAsync(ApplicationUser user);
    Task<RefreshTokenModel> CreateRefreshTokenAsync(ApplicationUser user, string? refreshToken = null);
    void RevokeAllAsync(ApplicationUser user);
}