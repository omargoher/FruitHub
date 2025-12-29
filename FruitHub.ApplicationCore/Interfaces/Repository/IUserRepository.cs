using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface IUserRepository : IGenericRepository<User, int>
{
    Task<User?> GetByIdentityUserIdAsync(string identityUserId);
    Task<User?> GetByIdentityUserIdWithCartAsync(string identityUserId);
}