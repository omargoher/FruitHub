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
    
    private IQueryable<Product> ApplySearching(IQueryable<Product> query, string search)
    {
        search = search.Trim().ToLower();
        query = query.Where(p =>
            p.Name.ToLower().Contains(search) ||
            p.Description.ToLower().Contains(search));

        return query;
    }

    private IQueryable<Product> ApplySorting(IQueryable<Product> query, ProductSortBy sortBy, SortDirection sortDir)
    {
        query = sortBy switch
        {
            ProductSortBy.Name => (sortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),

            ProductSortBy.Price => (sortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.Price)
                : query.OrderByDescending(p => p.Price),

            ProductSortBy.ExpirationPeriod => (sortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.ExpirationPeriodByDays)
                : query.OrderByDescending(p => p.ExpirationPeriodByDays),
            
            ProductSortBy.Calories => (sortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.Calories)
                : query.OrderByDescending(p => p.Calories),
            
            ProductSortBy.CreatedAt => (sortDir == SortDirection.Asc)
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            
            ProductSortBy.MostSelling => 
                query.OrderByDescending(p => p.OrderItems.Sum(oi => oi.Quantity)), 
            
            _ => query.OrderBy(p => p.Id)
        };

        return query;
    }

    private IQueryable<Product> ApplyPagination(IQueryable<Product> query, int? offset, int? limit)
    {
        if(offset.HasValue) query = query.Skip(offset.Value);
        if(limit.HasValue) query = query.Take(limit.Value);
        return query;
    }

    private IQueryable<Product> ApplyProductQuery(IQueryable<Product> query, ProductQuery productQuery)
    {
        if (!string.IsNullOrWhiteSpace(productQuery.Search))
        {
            query = ApplySearching(query, productQuery.Search);
        }

        if (productQuery.SortBy != null) query = ApplySorting(query, productQuery.SortBy.Value, productQuery.SortDir);

        query = ApplyPagination(query, productQuery.Offset, productQuery.Limit);

        return query;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetAllAsync(ProductQuery productQuery)
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

    public async Task<SingleProductResponseDto?> GetByIdWithCategoryNameAsync(int productId)
    {
        var product = await _context.Products
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
            }).SingleOrDefaultAsync(p => p.Id == productId);
        return product;
    }
    
    public async Task<IReadOnlyList<ProductResponseDto>> GetByCategoryIdAsync(int categoryId, ProductQuery productQuery)
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