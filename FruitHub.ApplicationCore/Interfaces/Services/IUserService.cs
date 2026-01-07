using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IUserService
{
    Task<UserProfileDto> GetByIdAsync(int userId);
    Task UpdateAsync(int userId, UpdateUserDto dto);

}