using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User, int>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) :base(context)
    {
    }

    public async Task<User?> GetByIdentityUserIdAsync(string identityUserId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserId == identityUserId);
    }
    
    public async Task<User?> GetByIdentityUserIdWithCartAsync(string identityUserId)
    {
        return await _context.Users
            .Include(u => u.Cart)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(u => u.UserId == identityUserId);
    }
}