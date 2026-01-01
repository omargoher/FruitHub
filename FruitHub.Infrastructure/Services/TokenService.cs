using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.Options;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FruitHub.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwt;
    
    public TokenService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtOptions> options)
    {
        _userManager = userManager;
        _jwt = options.Value;
    }

    public async Task<SecurityToken> GenerateJwtAsync(ApplicationUser user)
    {
        // Create user claims
        var userClaims = await _userManager.GetClaimsAsync(user) ?? new List<Claim>();
        var userRoles = await _userManager.GetRolesAsync(user) ?? new List<string>();
        
        foreach (var role in userRoles)
        {
            userClaims.Add(new Claim(ClaimTypes.Role, role));
        }
        userClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        userClaims.Add(new Claim(ClaimTypes.Name, user.UserName));
        userClaims.Add(new Claim(ClaimTypes.Email, user.Email));
        
        // prepare signingCredentials
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwt.SigningKey)
            ), SecurityAlgorithms.HmacSha256
        );

        // tokenDescriptor Contains some information which used to create a security token.
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            Expires = DateTime.Now.AddMinutes(_jwt.Lifetime),
            SigningCredentials = signingCredentials,
            Subject = new ClaimsIdentity(userClaims)
        };
        
        // create token
        // A SecurityTokenHandler designed for creating and validating Json Web Tokens.
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var securityToken = tokenHandler.CreateToken(tokenDescriptor); // create token with info in tokenDescriptor
        return securityToken;
    }

    public async Task<RefreshTokenDto> CreateRefreshTokenAsync(ApplicationUser user)
    {
        // Enforce single active session per user (security policy)
        RevokeAllAsync(user);
        
        var token = GenerateRefreshToken();

        var refreshToken = new RefreshTokenDto()
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(14),
        };
        
        user.RefreshTokens.Add(new RefreshToken
        {
            Token = token,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshToken.ExpiresAt
        });
        await _userManager.UpdateAsync(user);

        return refreshToken;
    }
    
    public void RevokeAllAsync(ApplicationUser user)
    {
        var activeTokens = user.RefreshTokens.Where(t => t.IsActive);
        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }
    }
    
    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }
}