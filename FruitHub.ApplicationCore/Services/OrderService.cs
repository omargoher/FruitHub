using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Interfaces.Repository;
using FruitHub.ApplicationCore.Interfaces.Services;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _uow;
    private readonly IProductRepository _productRepo;
    private readonly IUserRepository _userRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly ICartRepository _cartRepo;

    public OrderService(IUnitOfWork uow)
    {
        _uow = uow;
        _productRepo = uow.Product;
        _userRepo = uow.User;
        _orderRepo = uow.Order;
        _cartRepo = uow.Cart;
    }

    // TODO Filtering, Sorting, Paggination
    public async Task<IReadOnlyList<OrderResponseDto>> GetAllForUserAsync(string identityUserId, OrderQuery query)
    {
        var user = await _userRepo.GetByIdentityUserIdAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return await _orderRepo.GetOrdersForUser(user.Id, query);
    }

    public async Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(OrderQuery query)
    {
        return await _orderRepo.GetAll(query);
    }
    
    public async Task<OrderResponseDto?> GetByIdAsync(string identityUserId, string role, int orderId)
    {
        if(role == "Admin") return await _orderRepo.GetById(orderId);
        
        // Get User
        var user = await _userRepo.GetByIdentityUserIdAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }
        
        var order = await _orderRepo.GetById(orderId);

        if (order == null) return order;
        
        if (order.UserId != user.Id)
        {
            // may create custom exception 
            throw new UnauthorizedAccessException("You are not allowed to access this order");
        }

        return order;
    }
    
    public async Task Checkout(string identityUserId, CheckoutDto dto)
    {
        // Get User
        var user = await _userRepo.GetByIdentityUserIdAsync(identityUserId);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Get Cart
        var cart = await _cartRepo.GetWithCartItemsAndProductsAsync(user.Id);

        if(cart == null || !cart.Items.Any())
        {
            throw new InvalidOperationException("Cart is empty");
        }

        // Create Order
        var order = new Order();
        order.UserId = user.Id;
        
        foreach (var item in cart.Items)
        {
            if (item.Product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"Only Avaliable for {item.Product.Name}  {item.Product.Stock} pieces");
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
        order.ShippingFees = 30.0m; // => this hardcode now but 
        order.IsShipped = false;
        order.IsPayed = false; // => user pay when dreven order  

        order.SubPrice = cart.Items.Sum(i => i.Quantity * i.Product.Price);
        order.TotalPrice = order.SubPrice + order.ShippingFees;

        // save order
        _orderRepo.Add(order);
        
        // Clear Cart
        cart.Items.Clear();
        
        await _uow.SaveChangesAsync();
    }

    // User will pay when deliver order in current business logic 
    public async Task MarkOrderIsAShipped(int orderId)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);

        if (order == null)
            throw new KeyNotFoundException("Order not found");
        
        if (order.IsShipped)
            throw new InvalidOperationException("Order is already shipped");

        order.IsShipped = true;
        order.IsPayed = true;

        _orderRepo.Update(order);
        await _uow.SaveChangesAsync();
    }
}