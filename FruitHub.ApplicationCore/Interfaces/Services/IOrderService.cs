using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.DTOs.Order;

namespace FruitHub.ApplicationCore.Interfaces.Services;

public interface IOrderService
{
    Task<IReadOnlyList<OrderResponseDto>> GetAllForUserAsync(int userId, OrderQuery query);
    Task<IReadOnlyList<OrderResponseDto>> GetAllAsync(OrderQuery query);
    Task<OrderResponseDto?> GetByIdAsync(int orderId);
    Task<OrderResponseDto?> GetByIdAsync(int userId, int orderId);
    Task CheckoutAsync(int userId, CheckoutDto dto);
    Task ChangeOrderStatusAsync(int orderId, ChangeOrderStatusDto dto);
    Task CancelOrderAsync(int orderId);
    Task CancelOrderAsync(int userId, int orderId);
}