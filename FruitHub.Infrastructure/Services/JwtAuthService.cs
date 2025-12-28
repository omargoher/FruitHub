using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Options;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FruitHub.Infrastructure.Services;
public class JwtAuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _repositories;
    private readonly ITokenService _tokenService;
    public JwtAuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork repositories,
        ITokenService tokenService
        )
    {
        _userManager = userManager;
        _repositories = repositories;
        _tokenService = tokenService;
    }
    
    public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        ArgumentNullException.ThrowIfNull(registerDto);
        
        ApplicationUser? authUser = null;
        RegisterResponseDto response = new RegisterResponseDto();
        
        try
        {
            // Prop in dto controller validate it
            
            authUser = new ApplicationUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email
            };
           
            var identityResult = await _userManager.CreateAsync(authUser, registerDto.Password);

           
            if (!identityResult.Succeeded)
            {
                response.Errors = MapRegistrationIdentityErrors(identityResult.Errors);
                return response;
            }

            identityResult = await _userManager.AddToRoleAsync(authUser, "User");

            if (!identityResult.Succeeded)
            {
                response.Errors = MapRegistrationIdentityErrors(identityResult.Errors);
                await _userManager.DeleteAsync(authUser);
                return response;
            }
            
            var user = new User
            {
                UserId = authUser.Id,
                Email = authUser.Email,
                FullName = registerDto.FullName
            };
            
            _repositories.User.Add(user);

            var result = await _repositories.SaveChangesAsync();
            if (result == 0)
            {
                response.Errors.Add("Registration failed");
                await _userManager.DeleteAsync(authUser);
                return response;
            }
            
            response.FullName = user.FullName;
            response.UserName = authUser.UserName;
            response.Email = user.Email;
            response.IsRegistered = true;
            
            return response;
        }
        catch
        {
            if (authUser != null)
                await _userManager.DeleteAsync(authUser);

            throw;
        }
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        ArgumentNullException.ThrowIfNull(loginDto);
        var response = new LoginResponseDto();

        // var user = await _userManager.FindByEmailAsync(loginDto.Email);
        var user = await _userManager.Users.Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Email == loginDto.Email);

        if (user == null)
        {
            response.Errors.Add("This email not for any user");
            return response;
        }

        if (!await _userManager.CheckPasswordAsync(user, loginDto.Password))
        {
            response.Errors.Add("Password Is Incorrect");
            return response;
        }

        // how frontend know error is email is not verify
        if (!user.EmailConfirmed)
        {
            response.Errors.Add("Email is not verify");
            return response;
        }

        // Generate JWT
        var jwtToken = await _tokenService.GenerateJwtAsync(user);

        // Generate Refresh Token
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user);

        response.Email = user.Email;
        response.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        response.TokenExpiresAt = jwtToken.ValidTo;
        response.RefreshToken = refreshToken.Token;
        response.RefreshExpiresAt = refreshToken.ExpiresAt;
        response.IsAuthenticated = true;

        return response;
    }

    public async Task<LoginResponseDto> RefreshAsync(RefreshTokenRequestDto refreshToken)
    {
        ArgumentNullException.ThrowIfNull(refreshToken);
        var response = new LoginResponseDto();
        
       var user = await _userManager.Users
           .Include(u => u.RefreshTokens)
           .FirstOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken.RefreshToken));

       if (user == null)
       {
           response.Errors.Add("INVALID_TOKEN");
           return response;
       }
        
       var oldToken = user.RefreshTokens
           .Single(t => t.Token == refreshToken.RefreshToken);

       if (oldToken.IsExpired || oldToken.IsRevoked)
       {
           response.Errors.Add("INVALID_TOKEN");
           return response;
       }
       
       // Generate JWT
       var jwtToken = await _tokenService.GenerateJwtAsync(user);

       // Generate Refresh Token
       var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user);

       response.Email = user.Email;
       response.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
       response.TokenExpiresAt = jwtToken.ValidTo;
       response.RefreshToken = newRefreshToken.Token;
       response.RefreshExpiresAt = newRefreshToken.ExpiresAt;
       response.IsAuthenticated = true;

       return response;
    }

    public async Task LogoutAsync(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);
        var user = await _userManager.Users.Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return;
        }
        await _tokenService.RevokeAllAsync(user);
    }
   
    private static List<string> MapRegistrationIdentityErrors(IEnumerable<IdentityError> errors)
    {
        var result = new List<string>();

        foreach (var error in errors)
        {
            if (error.Code.Contains("Password"))
                result.Add(error.Description);
            else if (error.Code.Contains("DuplicateUserName"))
                result.Add("Username already exists");
            else if (error.Code.Contains("DuplicateEmail"))
                result.Add("Email already exists");
            else
                result.Add("Registration failed");
        }

        return result;
    }
}