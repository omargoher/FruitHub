using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Enums.Order;
using FruitHub.ApplicationCore.Enums;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Models;
using Microsoft.EntityFrameworkCore;

namespace FruitHub.Infrastructure.Persistence.Repositories;

public class OrderRepository : GenericRepository<Order, int>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) :base(context)
    {
    }

    private IQueryable<Order> ApplyFilters(IQueryable<Order> query, OrderFilterDto filter)
    {
        if (filter.Status.HasValue)
        {
            query = filter.Status switch
            {
                OrderStatus.Pending =>
                    query.Where(o => !o.IsPayed && !o.IsShipped),

                OrderStatus.Paid =>
                    query.Where(o => o.IsPayed),

                OrderStatus.Shipped =>
                    query.Where(o => o.IsShipped),

                OrderStatus.Cancelled =>
                    query.Where(o => o.IsCanceled),

                _ => query
            };
        }

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);
        
        if (filter.ToDate.HasValue)
            query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

        if (filter.UserId.HasValue)
            query = query.Where(o => o.UserId == filter.UserId.Value);

        return query;
    }
    
    private IQueryable<Order> ApplySort(IQueryable<Order> query, OrderSortBy sortBy, SortDirection sortDir)
    {
        query = sortBy switch
        {
            OrderSortBy.TotalPrice => (sortDir == SortDirection.Asc)
                ? query.OrderBy(o => o.TotalPrice)
                : query.OrderByDescending(o => o.TotalPrice),
            
            OrderSortBy.SubPrice => (sortDir == SortDirection.Asc)
                ? query.OrderBy(o => o.SubPrice)
                : query.OrderByDescending(o => o.SubPrice),
            
            OrderSortBy.ShippingFees => (sortDir == SortDirection.Asc)
                ? query.OrderBy(o => o.ShippingFees)
                : query.OrderByDescending(o => o.ShippingFees),
            
            OrderSortBy.Status => (sortDir == SortDirection.Asc)
                ? query.OrderBy(o => o.IsPayed).ThenBy(o => o.IsShipped)
                : query.OrderByDescending(o => o.IsPayed).ThenByDescending(o => o.IsShipped),
            
            _ => query.OrderBy(o => o.Id)
        };
        return query;
    }
    
    private IQueryable<Order> ApplyPagination(IQueryable<Order> query, int? offset, int? limit)
    {
        if(offset.HasValue) query = query.Skip(offset.Value);
        if(limit.HasValue) query = query.Take(limit.Value);
        return query;
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetAllWithOrderItemsAsync(OrderQuery orderQuery)
    {
        IQueryable<Order> query = _context.Orders;

        if (orderQuery.Filter != null) query = ApplyFilters(query, orderQuery.Filter);
        if (orderQuery.SortBy != null) query = ApplySort(query, orderQuery.SortBy.Value, orderQuery.SortDir);
        query = ApplyPagination(query, orderQuery.Offset, orderQuery.Limit);

        return await query.Select(o => new OrderResponseDto
        {
            OrderId = o.Id,
            UserId = o.UserId,
            CustomerFullName = o.CustomerFullName,
            CustomerAddress = o.CustomerAddress,
            CustomerCity = o.CustomerCity,
            CustomerDepartment = o.CustomerDepartment,
            CustomerPhoneNumber = o.CustomerPhoneNumber,
            SubPrice = o.SubPrice,
            TotalPrice = o.TotalPrice,
            ShippingFees = o.ShippingFees,
            IsShipped = o.IsShipped,
            IsPayed = o.IsPayed,
            IsCanceled = o.IsCanceled,
            Items = o.Items.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                ProductName = oi.Product.Name,
                PricePerPiece = oi.PricePerPiece
            }).ToList()
        }).ToListAsync();
    }
    
    public async Task<IReadOnlyList<OrderResponseDto>> GetByUserIdWithOrderItemsAsync(int userId, OrderQuery orderQuery)
    {
        IQueryable<Order> query = _context.Orders.Where(o => o.UserId == userId);

        if (orderQuery.Filter != null) query = ApplyFilters(query, orderQuery.Filter);
        if (orderQuery.SortBy != null) query = ApplySort(query, orderQuery.SortBy.Value, orderQuery.SortDir);
        query = ApplyPagination(query, orderQuery.Offset, orderQuery.Limit);

        return await query.Select(o => new OrderResponseDto
        {
            OrderId = o.Id,
            UserId = o.UserId,
            CustomerFullName = o.CustomerFullName,
            CustomerAddress = o.CustomerAddress,
            CustomerCity = o.CustomerCity,
            CustomerDepartment = o.CustomerDepartment,
            CustomerPhoneNumber = o.CustomerPhoneNumber,
            SubPrice = o.SubPrice,
            TotalPrice = o.TotalPrice,
            ShippingFees = o.ShippingFees,
            IsShipped = o.IsShipped,
            IsPayed = o.IsPayed, 
            IsCanceled = o.IsCanceled,
            Items = o.Items.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                ProductName = oi.Product.Name,
                PricePerPiece = oi.PricePerPiece
            }).ToList()
        }).ToListAsync();
    }

    public async Task<OrderResponseDto?> GetByIdWithOrderItemsAsync(int orderId)
    {
        return await  _context.Orders.Where(o => o.Id == orderId)
            .Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                UserId = o.UserId,
                CustomerFullName = o.CustomerFullName,
                CustomerAddress = o.CustomerAddress,
                CustomerCity = o.CustomerCity,
                CustomerDepartment = o.CustomerDepartment,
                CustomerPhoneNumber = o.CustomerPhoneNumber,
                SubPrice = o.SubPrice,
                TotalPrice = o.TotalPrice,
                ShippingFees = o.ShippingFees,
                IsShipped = o.IsShipped,
                IsPayed = o.IsPayed,
                IsCanceled = o.IsCanceled,
                Items = o.Items.Select(oi => new OrderItemResponseDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    ProductName = oi.Product.Name,
                    PricePerPiece = oi.PricePerPiece
                }).ToList()
            }).FirstOrDefaultAsync();
    }
}