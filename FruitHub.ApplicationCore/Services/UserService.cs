using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepo;

    public UserService(IUnitOfWork uow)
    {
        _uow = uow;
        _userRepo = uow.User;
    }

    public async Task<UserProfileDto> GetByIdAsync(int userId)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new NotFoundException($"User with id {userId} not found");
        }
        
        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    public async Task UpdateAsync(int userId, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdAsync(userId);

        if (user == null)
        {
            throw new NotFoundException($"User with id {userId} not found");
        }
        
        user.FullName = dto.FullName ?? user.FullName;
        
        _userRepo.Update(user);
        await _uow.SaveChangesAsync();
    }
}