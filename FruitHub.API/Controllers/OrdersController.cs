using System.Security.Claims;
using FruitHub.API.Extensions;
using FruitHub.API.Responses;
using FruitHub.ApplicationCore.DTOs.Order;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

/// <summary>
/// Manages order operations such as listing, checkout, status updates, and cancellation.
/// </summary>
/// <remarks>
/// This controller provides endpoints for managing orders for both admins and users.
///
/// **Authorization:**
/// - Requires authenticated user.
/// - Access is restricted based on role (Admin / User).
/// </remarks>
/// <response code="401">User is not authenticated.</response>
/// <response code="403">User is not authorized to access this resource.</response>
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }
    
    /// <summary>
    /// Retrieves all orders in the system.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - Admin only.
    ///
    /// Supports filtering, sorting, and pagination via query parameters.
    /// </remarks>
    /// <param name="query">Order filtering and pagination options.</param>
    /// <response code="200">Orders retrieved successfully.</response>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery] OrderQuery query)
    {
        var orders = await _orderService.GetAllAsync(query);  
        return Ok(orders);
    }
    
    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - Admin only.
    /// </remarks>
    /// <param name="orderId">Order identifier.</param>
    /// <response code="200">Order retrieved successfully.</response>
    /// <response code="404">Order not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpGet("{orderId:int}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int orderId)
    {
        var order = await _orderService.GetByIdAsync(orderId);
        return Ok(order);
    }
    
    /// <summary>
    /// Updates the payment or shipping status of an order.
    /// </summary>
    /// <remarks>
    /// **Authorization:** Admin only.
    /// **Status:**
    /// - Pending   = 0, // Order created, not paid
    /// - Shipped   = 1, // Order handed to delivery
    /// - Delivered = 2, // Customer received order
    /// - Cancelled = 3  // Order cancelled (before shipping)  
    /// **Business rules:**
    /// - An order in **Pending** status can be:
    ///   - Shipped
    ///   - Cancelled
    /// - An order in **Shipped** status can only be:
    ///   - Delivered
    /// - Orders in any other status cannot transition to another state.
    /// </remarks>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="dto">Order status update data.</param>
    /// <response code="204">Order status updated successfully.</response>
    /// <response code="400">Invalid status transition.</response>
    /// <response code="404">Order not found.</response>
    [Authorize(Roles = "Admin")]
    [HttpPut("{orderId:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatusAsync(int orderId, UpdateOrderStatusDto dto)
    {
        await _orderService.UpdateStatusAsync(orderId, dto);
        return NoContent();
    }
    
    /// <summary>
    /// Retrieves orders belonging to the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - User only.
    ///
    /// Supports filtering, sorting, and pagination.
    /// </remarks>
    /// <param name="query">Order filtering and pagination options.</param>
    /// <response code="200">Orders retrieved successfully.</response>
    [Authorize(Roles = "User")]
    [HttpGet("my")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyOrdersAsync([FromQuery] OrderQuery query)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        var orders = await _orderService.GetAllAsync(userId, query);  
        return Ok(orders);
    }
    
    
    
    /// <summary>
    /// Retrieves a specific order belonging to the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - User only.
    /// </remarks>
    /// <param name="orderId">Order identifier.</param>
    /// <response code="200">Order retrieved successfully.</response>
    /// <response code="404">Order not found.</response>
    [Authorize(Roles = "User")]
    [HttpGet("my/{orderId:int}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyOrderByIdAsync(int orderId)
    {
            var userId = ClaimsPrincipalExtensions.GetUserId(User);
            var order = await _orderService.GetByIdAsync(userId, orderId);  

        return Ok(order);
    }
    
    /// <summary>
    /// Creates a new order from the user's cart.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - User only.
    ///
    /// **Business rules:**
    /// - Cart must not be empty.
    /// - Product stock is validated before checkout.
    /// </remarks>
    /// <param name="dto">Checkout information.</param>
    /// <response code="204">Order created successfully.</response>
    /// <response code="400">Invalid request or empty cart.</response>
    [Authorize(Roles = "User")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(CreateOrderDto dto)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
        var order = await _orderService.CreateAsync(userId, dto);
        
        return CreatedAtAction("GetMyOrderById",
            new {orderId = order.OrderId},
            order);
    }
    
    
    
    /// <summary>
    /// Cancels an order belonging to the authenticated user.
    /// </summary>
    /// <remarks>
    /// **Authorization:**
    /// - User only.
    ///
    /// **Rules:**
    /// - Order can only be cancelled while in **Pending** state.
    /// </remarks>
    /// <param name="orderId">Order identifier.</param>
    /// <response code="204">Order cancelled successfully.</response>
    /// <response code="400">Order cannot be cancelled.</response>
    /// <response code="404">Order not found.</response>
    [Authorize(Roles = "User")]
    [HttpDelete("my/{orderId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelAsync(int orderId)
    {
        var userId = ClaimsPrincipalExtensions.GetUserId(User);
            await _orderService.CancelAsync(userId, orderId);  
        return NoContent();
    }
}