using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;

public class 
    OrderService : IOrderService
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
    
    public async Task<IReadOnlyList<OrderResponseDto>> GetAllForUserAsync(int userId, OrderQuery query)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        return await _orderRepo.GetByUserIdWithOrderItemsAsync(userId, query);
    }


    public async Task<OrderResponseDto?> GetByIdAsync(int orderId)
    {
        return await _orderRepo.GetByIdWithOrderItemsAsync(orderId);
    }

    public async Task<OrderResponseDto?> GetByIdAsync(int userId, int orderId)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        var order = await _orderRepo.GetByIdWithOrderItemsAsync(orderId);

        if (order == null) return order;

        if (order.UserId != userId)
        {
            throw new ForbiddenException();
        }

        return order;
    }

    public async Task CheckoutAsync(int userId, CheckoutDto dto)
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
    }

    // The user pays when the order is delivered in current business logic 
    public async Task ChangeOrderStatusAsync(int orderId, ChangeOrderStatusDto dto)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
        {
            throw new NotFoundException("Order");
        }

        if (dto.IsShipped.HasValue && dto.IsPayed.HasValue)
        {
            if (dto.IsShipped.Value && !dto.IsPayed.Value)
            {
                throw new InvalidRequestException("Order Can not be shipped and not payed");
            }
        }

        if (dto.IsShipped.HasValue)
        {
            order.IsShipped = dto.IsShipped.Value;

            // Is order Is Shipped so should be Is Payed but 
            if (order.IsShipped)
            {
                order.IsPayed = true;
            }
        }
        if (dto.IsPayed.HasValue)
        {
            order.IsPayed = dto.IsPayed.Value;

            // if order not payed yet so can not be shipped
            if (!order.IsPayed)
            {
                order.IsShipped = false;
            }
        }

        _orderRepo.Update(order);
        await _uow.SaveChangesAsync();
    }

    public async Task CancelOrderAsync(int orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new NotFoundException("Order");

        order.IsCanceled = true;

        // Id order is canceled so order can not be shipped or payed 
        order.IsShipped = false;
        order.IsPayed = false;
        
        _orderRepo.Update(order);
        await _uow.SaveChangesAsync();
    }
    
    public async Task CancelOrderAsync(int userId, int orderId)
    {
        if (!await _userRepo.IsExistAsync(userId))
        {
            throw new NotFoundException("User");
        }

        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new NotFoundException("Order");

        if (order.UserId != userId)
        {
            throw new ForbiddenException();
        }
        
        order.IsCanceled = true;

        // Id order is canceled so order can not be shipped or payed 
        order.IsShipped = false;
        order.IsPayed = false;
        
        _orderRepo.Update(order);
        await _uow.SaveChangesAsync();
    }
}