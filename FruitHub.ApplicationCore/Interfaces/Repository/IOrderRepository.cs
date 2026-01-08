using FruitHub.ApplicationCore.DTOs.Cart;
using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Models;

namespace FruitHub.ApplicationCore.Interfaces.Repository;

public interface IOrderRepository : IGenericRepository<Order, int>
{
   Task<IReadOnlyList<OrderResponseDto>> GetByUserIdWithOrderItemsAsync(int userId, OrderQuery query);
   Task<IReadOnlyList<OrderResponseDto>> GetAllWithOrderItemsAsync(OrderQuery query);
   Task<OrderResponseDto?> GetByIdWithOrderItemsAsync(int orderId);
}