using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class UserRepository : GenericRepository<User, int>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) :base(context)
    {
    }
}