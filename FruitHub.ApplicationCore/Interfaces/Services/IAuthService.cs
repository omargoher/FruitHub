using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IAuthService
{
    Task<User> RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dtp);
    Task<LoginResponseDto> RefreshAsync(RefreshTokenRequestDto refreshToken);
    Task LogoutAsync(string userId);
}