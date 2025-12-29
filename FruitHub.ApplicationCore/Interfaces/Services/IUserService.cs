using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto> GetUserAsync(string identityUserId);
    Task UpdateUserAsync(string identityUserId, UpdateUserDto dto);

}