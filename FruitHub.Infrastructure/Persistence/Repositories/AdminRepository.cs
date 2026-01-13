using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class AdminRepository : GenericRepository<Admin, int>, IAdminRepository
{
    public AdminRepository(ApplicationDbContext context) :base(context)
    {
    }
}