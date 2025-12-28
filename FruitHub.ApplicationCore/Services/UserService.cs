using FruitHub.ApplicationCore.DTOs.User;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;

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

    public async Task<UserProfileDto> GetUserAsync(string userId)
    {
        var user = await _userRepo.GetByIdentityUserIdAsync(userId);
        
        if (user == null)
        {
            throw new KeyNotFoundException();
        }
        
        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };
    }

    public async Task UpdateUserAsync(string userId, UpdateUserDto dto)
    {
        var user = await _userRepo.GetByIdentityUserIdAsync(userId);

        if (user == null)
        {
            throw new KeyNotFoundException();
        }
        
        user.FullName = dto.FullName ?? user.FullName;
        
        _userRepo.Update(user);
        await _uow.SaveChangesAsync();
    }
}