using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface ICategoryRepository : IGenericRepository<Category, int>
{
    Task<bool> IsNameExistAsync(string categoryName);
}