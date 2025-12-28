using FruitHub.ApplicationCore.DTOs.User;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto> GetUserAsync(string userId);
    Task UpdateUserAsync(string userId, UpdateUserDto dto);
}