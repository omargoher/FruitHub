using FruitHub.ApplicationCore.DTOs.Product;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product, int>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) :base(context)
    {
    }

    private IQueryable<Product> ApplyProductQuery(IQueryable<Product> query, ProductQuery productQuery)
    {
        if (!string.IsNullOrWhiteSpace(productQuery.Search))
        {
            var search = productQuery.Search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Description.ToLower().Contains(search));
        }

        query = productQuery.SortBy switch
        {
            ProductSortBy.Name => (productQuery.SortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),

            ProductSortBy.Price => (productQuery.SortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.Price)
                : query.OrderByDescending(p => p.Price),

            ProductSortBy.ExpirationPeriod => (productQuery.SortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.ExpirationPeriodByDays)
                : query.OrderByDescending(p => p.ExpirationPeriodByDays),
            
            ProductSortBy.Calories => (productQuery.SortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.Calories)
                : query.OrderByDescending(p => p.Calories),
            
            ProductSortBy.MostSelling => 
                query.OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity)), 
            
            _ => query.OrderBy(p => p.Id)
        };

        if (productQuery.Offset.HasValue)
        {
            query = query.Skip(productQuery.Offset.Value);
        }
        
        if (productQuery.Limit.HasValue)
        {
            query = query.Take(productQuery.Limit.Value);
        }

        return query;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetProductsAsync(ProductQuery productQuery)
    {
        // i am not need send categories with response so not include it in queries 
        IQueryable<Product> query = _context.Products
            .AsNoTracking();

        query = ApplyProductQuery(query, productQuery);

        return await query.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            ImagePath = p.ImagePath
        }).ToListAsync();
    }

    public async Task<SingleProductResponseDto?> GetProductByIdWithCategoryAsync(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Select(p => new SingleProductResponseDto 
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Calories = p.Calories,
                Description = p.Description,
                Organic = p.Organic,
                ExpirationPeriodByDays = p.ExpirationPeriodByDays,
                Stock = p.Stock,
                ImagePath = p.ImagePath,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                
            }).SingleOrDefaultAsync(p => p.Id == id);
        return product;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetByCategoryAsync(int categoryId, ProductQuery productQuery)
    {
        IQueryable<Product> query = _context.Products
            .AsNoTracking();

        query = ApplyProductQuery(query, productQuery);
        
        // i am not need send categories with response so not include it in queries 
        query = query.Where(p => p.CategoryId == categoryId);
    
        return await query.Select(p => new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            ImagePath = p.ImagePath
        }).ToListAsync();
    }
 
    public async Task<bool> CheckIfProductExist(int productId)
    {
        return await _context.Products
            .AnyAsync(p => p.Id == productId);
    }
}