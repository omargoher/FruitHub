using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Enums.Order;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;


public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IUserRepository _userRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly ICartRepository _cartRepo;

    public OrderService(IUnitOfWork uow)
    {
        _uow = uow;
        _userRepo = uow.User;
        _orderRepo = uow.Order;
        _cartRepo = uow.Cart;
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(OrderQuery query)
    {
        return await _orderRepo.GetAllWithOrderItemsAsync(query);
    }
    
    public async Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(int userId, OrderQuery query)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        return await _orderRepo.GetByUserIdWithOrderItemsAsync(userId, query);
    }


    public async Task<OrderResponseDto?> GetByIdAsync(int orderId)
    {
        var order =  await _orderRepo.GetByIdWithOrderItemsAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Order");
        }
        return order;
    }

    public async Task<OrderResponseDto?> GetByIdAsync(int userId, int orderId)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        var order = await _orderRepo.GetByIdWithOrderItemsAsync(orderId);

        if (order == null)
        {
            throw new NotFoundException("Order");
        }
        
        if (order.UserId != userId)
        {
            throw new ForbiddenException();
        }

        return order;
    }

    public async Task<OrderResponseDto> CreateAsync(int userId, CreateOrderDto dto)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        var cart = await _cartRepo.GetByUserIdWithCartItemsAndProductsAsync(userId);
        if(cart == null || !cart.Items.Any())
        {
            throw new InvalidRequestException("Cart is empty");
        }

        var order = new Order();
        order.UserId = userId;
        
        foreach (var item in cart.Items)
        {
            if (item.Product.Stock < item.Quantity)
            {
                throw new InvalidRequestException($"Only {item.Product.Stock} items available for {item.Product.Name}");
            }
            
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                PricePerPiece = item.Product.Price,
                Product = item.Product,
            });

            item.Product.Stock -= item.Quantity;
        }
        order.CustomerFullName = dto.CustomerFullName;
        order.CustomerAddress = dto.CustomerAddress;
        order.CustomerCity = dto.CustomerCity;
        order.CustomerDepartment = dto.CustomerDepartment;
        order.CustomerPhoneNumber = dto.CustomerPhoneNumber;
        order.ShippingFees = 30.0m; // => currently this hardcode
        // => The user pays when the order is delivered. 

        order.SubPrice = cart.Items.Sum(i => i.Quantity * i.Product.Price);
        order.TotalPrice = order.SubPrice + order.ShippingFees;

        _orderRepo.Add(order);
        
        cart.Items.Clear();
        
        await _uow.SaveChangesAsync();

        return new OrderResponseDto
        {
            OrderId = order.Id,
            UserId = order.UserId,
            CustomerFullName = order.CustomerFullName,
            CustomerAddress = order.CustomerAddress,
            CustomerCity = order.CustomerCity,
            CustomerDepartment = order.CustomerDepartment,
            CustomerPhoneNumber = order.CustomerPhoneNumber,
            SubPrice = order.SubPrice,
            TotalPrice = order.TotalPrice,
            ShippingFees = order.ShippingFees,
            OrderStatus = order.OrderStatus,
            Items = order.Items.Select(oi => new OrderItemResponseDto
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                ProductName = oi.Product.Name,
                PricePerPiece = oi.PricePerPiece
            }).ToList()
        };
    }
    
    public async Task UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
        {
            throw new NotFoundException("Order");
        }
        
        var currentStatus = order.OrderStatus;
        var targetStatus = dto.OrderStatus;
        
        if (currentStatus == targetStatus)
        {
            return;
        }

        order.ChangeStatus(targetStatus);

        _orderRepo.Update(order);
        await _uow.SaveChangesAsync();
    }
    
    public async Task CancelAsync(int userId, int orderId)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
        {
            throw new NotFoundException("Order");
        }
        
        if (order.UserId != userId)
        {
            throw new ForbiddenException();
        }

        order.Cancel();
        
        _orderRepo.Update(order);
        await _uow.SaveChangesAsync();
    }
}