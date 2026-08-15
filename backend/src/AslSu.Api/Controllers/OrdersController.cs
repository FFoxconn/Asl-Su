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
public class OrdersController(IOrderService orderService, IOrderWorkflowService orderWorkflowService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        Ok(await orderService.GetAllAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await orderService.GetByIdAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{id:int}/accept")]
    public Task<IActionResult> Accept(int id, CancellationToken cancellationToken) =>
        RespondAsync(orderWorkflowService.AcceptAsync(id, cancellationToken));

    [HttpPost("{id:int}/start-preparing")]
    public Task<IActionResult> StartPreparing(int id, CancellationToken cancellationToken) =>
        RespondAsync(orderWorkflowService.StartPreparingAsync(id, cancellationToken));

    [HttpPost("{id:int}/mark-prepared")]
    public Task<IActionResult> MarkPrepared(int id, CancellationToken cancellationToken) =>
        RespondAsync(orderWorkflowService.MarkPreparedAsync(id, cancellationToken));

    [HttpPost("{id:int}/deliver")]
    public Task<IActionResult> Deliver(int id, CancellationToken cancellationToken) =>
        RespondAsync(orderWorkflowService.DeliverAsync(id, cancellationToken));

    [HttpPut("{id:int}/courier")]
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

        return result.Error == OrderWorkflowError.NotFound
            ? NotFound()
            : Problem("Sipariş bu adımda değil veya bu geçiş şu anda geçerli değil.", statusCode: StatusCodes.Status409Conflict);
    }
}
