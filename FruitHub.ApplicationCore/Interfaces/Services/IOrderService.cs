using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.DTOs.Order;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponseDto>> GetAllForUserAsync(string identityUserId, OrderQuery query);
    Task Checkout(string identityUserId, CheckoutDto dto);
    Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(OrderQuery query);
    Task<OrderResponseDto?> GetByIdAsync(string identityUserId, string role, int orderId);
    Task MarkOrderIsAShipped(int orderId);
}