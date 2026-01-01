using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IAuthService
{
    Task RegisterAsync(RegisterDto dto);
    Task<LoginResponseDto> LoginAsync(LoginDto dtp);
    Task<LoginResponseDto> RefreshAsync(RefreshTokenRequestDto refreshToken);
    Task LogoutAsync(string userId);
}