using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace FruitHub.Infrastructure.Services;
public class JwtAuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IIdentityUserRepository _identityUserRepo;
    private readonly ITokenService _tokenService;
    public JwtAuthService(
        IUnitOfWork uow,
        ITokenService tokenService,
        IIdentityUserRepository identityUserRepo
        )
    {
        _uow = uow;
        _tokenService = tokenService;
        _identityUserRepo = identityUserRepo;
    }
    
    public async Task RegisterAsync(RegisterDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);
        
        ApplicationUser? identityUser = null;
        
        try
        {
            identityUser = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };
            
            var identityResult = await _identityUserRepo.CreateAsync(identityUser, registerDto.Password);
           
            if (!identityResult.Succeeded)
            {
                throw new IdentityOperationException(
                    identityResult.Errors.Select(e => e.Description));
            }

            identityResult = await _identityUserRepo.AddToRoleAsync(identityUser, "User");

            if (!identityResult.Succeeded)
            {
                throw new IdentityOperationException(
                    identityResult.Errors.Select(e => e.Description));
            }
            
            var businessUser = new User
            {
                UserId = identityUser.Id,
                Email = identityUser.Email,
                FullName = registerDto.FullName
            };
            
            _uow.User.Add(businessUser);

            var saved = await _uow.SaveChangesAsync();
            
            if (saved == 0)
            {
                throw new RegistrationFailedException("Failed to create business user");
            }

            identityResult = await _identityUserRepo.AddClaimAsync(identityUser, new Claim(
                "business_user_id",
                businessUser.Id.ToString()
            ));
            
            if (!identityResult.Succeeded)
            {
                throw new IdentityOperationException(
                    identityResult.Errors.Select(e => e.Description));
            }
        }
        catch
        {
            if (identityUser != null)
                await _identityUserRepo.DeleteAsync(identityUser);

            throw;
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);
        var response = new LoginResponseDto();

        var user = await _identityUserRepo.GetByEmailWithRefreshTokens(loginDto.Email);

        if (user == null || !await _identityUserRepo.CheckPasswordAsync(user, loginDto.Password))
        { 
            throw new InvalidCredentialsException();
        }

        if (!user.EmailConfirmed)
        {
            throw new EmailNotConfirmedException();
        }

        var jwtToken = await _tokenService.GenerateJwtAsync(user);

        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user);

        response.Email = user.Email;
        response.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        response.TokenExpiresAt = jwtToken.ValidTo;
        response.RefreshToken = refreshToken.Token;
        response.RefreshExpiresAt = refreshToken.ExpiresAt;

        return response;
    }

    public async Task<LoginResponseDto> RefreshAsync(RefreshTokenRequestDto refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        var response = new LoginResponseDto();

        var user = await _identityUserRepo.GetByRefreshTokenWithRefreshTokens(refreshToken.RefreshToken);

       if (user == null)
       {
           throw new InvalidRefreshTokenException();
       }
        
       var oldToken = user.RefreshTokens
           .FirstOrDefault(t => t.Token == refreshToken.RefreshToken);

       if (oldToken == null || oldToken.IsExpired || oldToken.IsRevoked)
       {
           throw new InvalidRefreshTokenException();
       }
       
       var jwtToken = await _tokenService.GenerateJwtAsync(user);

       var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user);

       response.Email = user.Email;
       response.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
       response.TokenExpiresAt = jwtToken.ValidTo;
       response.RefreshToken = newRefreshToken.Token;
       response.RefreshExpiresAt = newRefreshToken.ExpiresAt;

       return response;
    }

    public async Task LogoutAsync(string identityUserId)
    {
        ArgumentNullException.ThrowIfNull(identityUserId);

        var user = await _identityUserRepo.GetByIdWithRefreshTokens(identityUserId);

        if (user == null)
        {
            return;
        }
        _tokenService.RevokeAllAsync(user);
        
        var identityResult = await _identityUserRepo.UpdateAsync(user);
        
        if (!identityResult.Succeeded)
        {
            throw new IdentityOperationException(
                identityResult.Errors.Select(e => e.Description));
        }
    }
   
    private static string MapRegistrationIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var result="";

        foreach (var error in errors)
        {
            if (error.Code.Contains("Password"))
                result=error.Description;
            else if (error.Code.Contains("DuplicateUserName"))
                result="Username already exists";
            else if (error.Code.Contains("DuplicateEmail"))
                result="Email already exists";
            else
                result="Registration failed";
        }

        return result;
    }
}