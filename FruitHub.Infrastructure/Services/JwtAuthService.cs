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
    
    public async Task<User> RegisterAsync(RegisterDto registerDto)
    {
        ApplicationUser? identityUser = null;
        
        try
        {
            // 1. Identity creation
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

            // 2. Role assignment
            identityResult = await _identityUserRepo.AddToRoleAsync(identityUser, "User");

            if (!identityResult.Succeeded)
            {
                throw new IdentityOperationException(
                    identityResult.Errors.Select(e => e.Description));
            }
            
            // 3. Business user creation
            var businessUser = new User
            {
                UserId = identityUser.Id,
                Email = identityUser.Email,
                FullName = registerDto.FullName
            };
            
            _uow.User.Add(businessUser);

            var saved = await _uow.SaveChangesAsync();
            //
            // if (saved == 0)
            // {
            //     throw new RegistrationFailedException("Failed to create user");
            // }

            // 4. Claims linking
            identityResult = await _identityUserRepo.AddClaimAsync(identityUser, new Claim(
                "business_user_id",
                businessUser.Id.ToString()
            ));
            if (!identityResult.Succeeded)
            {
                throw new IdentityOperationException(
                    identityResult.Errors.Select(e => e.Description));
            }

            return businessUser;
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
        var response = new LoginResponseDto();

        var user = await _identityUserRepo.GetByEmail(loginDto.Email);

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
        var response = new LoginResponseDto();

        var user = await _identityUserRepo.GetByRefreshTokenWithRefreshTokens(refreshToken.RefreshToken);

       if (user == null)
       {
           throw new InvalidRefreshTokenException();
       }
        
       var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user, refreshToken.RefreshToken);

       var jwtToken = await _tokenService.GenerateJwtAsync(user);

       response.Email = user.Email;
       response.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
       response.TokenExpiresAt = jwtToken.ValidTo;
       response.RefreshToken = newRefreshToken.Token;
       response.RefreshExpiresAt = newRefreshToken.ExpiresAt;

       return response;
    }

    public async Task LogoutAsync(string identityUserId)
    {
        var user = await _identityUserRepo.GetByIdWithRefreshTokens(identityUserId);

        if (user == null)
        {
            return;
        }
        _tokenService.RevokeAllAsync(user);
        
        await _identityUserRepo.UpdateAsync(user);
    }
}