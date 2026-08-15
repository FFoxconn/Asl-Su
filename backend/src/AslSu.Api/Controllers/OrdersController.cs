using AslSu.Application.OrderWorkflow;
using AslSu.Application.OrderWorkflow.Dtos;
using AslSu.Application.Orders;
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
