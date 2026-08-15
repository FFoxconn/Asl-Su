using AslSu.Application.Orders.Dtos;

namespace AslSu.Application.Orders;

public interface IOrderService
{
    Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
