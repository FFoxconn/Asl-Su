using System.Security.Claims;
using AslSu.Application.Couriers;
using AslSu.Application.OrderWorkflow;
using AslSu.Application.OrderWorkflow.Dtos;
using AslSu.Application.Orders;
using AslSu.Application.Orders.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AslSu.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController(
    IOrderService orderService, IOrderWorkflowService orderWorkflowService, ICourierService courierService)
    : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await orderService.GetAllAsync(cancellationToken: cancellationToken));

    /// <summary>Courier-only: this courier's own assigned orders, most recent first.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Courier")]
    public async Task<IActionResult> GetMy(CancellationToken cancellationToken)
    {
        var courier = await courierService.GetByUserIdAsync(GetCurrentUserId(), cancellationToken);
        if (courier is null)
        {
            return Ok(Array.Empty<OrderListItemDto>());
        }

        return Ok(await orderService.GetAllAsync(courier.Id, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (IsCourier() && !await IsOwnOrderAsync(order.CourierId, cancellationToken))
        {
            return NotFound();
        }

        return Ok(order);
    }

    [HttpPost("{id:int}/accept")]
    [Authorize(Roles = "Admin,Operator")]
    public Task<IActionResult> Accept(int id, CancellationToken cancellationToken) =>
        RespondAsync(orderWorkflowService.AcceptAsync(id, cancellationToken));

    [HttpPost("{id:int}/start-preparing")]
    [Authorize(Roles = "Admin,Operator")]
    public Task<IActionResult> StartPreparing(int id, CancellationToken cancellationToken) =>
        RespondAsync(orderWorkflowService.StartPreparingAsync(id, cancellationToken));

    [HttpPost("{id:int}/mark-prepared")]
    [Authorize(Roles = "Admin,Operator")]
    public Task<IActionResult> MarkPrepared(int id, CancellationToken cancellationToken) =>
        RespondAsync(orderWorkflowService.MarkPreparedAsync(id, cancellationToken));

    /// <summary>Admin/Operator can deliver any order; a Courier can only deliver orders
    /// assigned to them (enforced server-side, not just hidden client-side).</summary>
    [HttpPost("{id:int}/deliver")]
    public async Task<IActionResult> Deliver(int id, CancellationToken cancellationToken)
    {
        int? requiredCourierId = null;
        if (IsCourier())
        {
            var courier = await courierService.GetByUserIdAsync(GetCurrentUserId(), cancellationToken);
            if (courier is null)
            {
                return Forbid();
            }

            requiredCourierId = courier.Id;
        }

        return await RespondAsync(orderWorkflowService.DeliverAsync(id, requiredCourierId, cancellationToken));
    }

    [HttpPut("{id:int}/courier")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> AssignCourier(int id, AssignCourierRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.AssignCourierAsync(id, request.CourierId, cancellationToken);
        if (result.Success)
        {
            return Ok(await orderService.GetByIdAsync(id, cancellationToken));
        }

        return result.Error == OrderAssignCourierError.OrderNotFound
            ? NotFound("Sipariş bulunamadı.")
            : NotFound("Kurye bulunamadı.");
    }

    [HttpPut("{orderId:int}/items/{itemId:int}/substitute")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> SubstituteItem(
        int orderId, int itemId, SubstituteOrderItemRequest request, CancellationToken cancellationToken)
    {
        var result = await orderService.SubstituteOrderItemAsync(orderId, itemId, request.ProductId, cancellationToken);
        if (result.Success)
        {
            return Ok(await orderService.GetByIdAsync(orderId, cancellationToken));
        }

        return result.Error switch
        {
            OrderItemSubstituteError.OrderNotFound => NotFound("Sipariş bulunamadı."),
            OrderItemSubstituteError.ItemNotFound => NotFound("Sipariş kalemi bulunamadı."),
            _ => NotFound("İkame ürün bulunamadı."),
        };
    }

    private async Task<IActionResult> RespondAsync(Task<OrderWorkflowActionResult> action)
    {
        var result = await action;
        if (result.Success)
        {
            return Ok(result);
        }

        return result.Error switch
        {
            OrderWorkflowError.NotFound => NotFound(),
            OrderWorkflowError.Forbidden => Forbid(),
            _ => Problem("Sipariş bu adımda değil veya bu geçiş şu anda geçerli değil.", statusCode: StatusCodes.Status409Conflict),
        };
    }

    private bool IsCourier() => User.FindFirstValue(ClaimTypes.Role) == "Courier";

    private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task<bool> IsOwnOrderAsync(int? orderCourierId, CancellationToken cancellationToken)
    {
        if (!orderCourierId.HasValue)
        {
            return false;
        }

        var courier = await courierService.GetByUserIdAsync(GetCurrentUserId(), cancellationToken);
        return courier is not null && courier.Id == orderCourierId.Value;
    }
}
