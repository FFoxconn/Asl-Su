using AslSu.Application.Orders.Dtos;

namespace AslSu.Application.Orders;

public interface IOrderService
{
    /// <summary>Pass <paramref name="courierId"/> to scope results to a single courier's
    /// assigned orders (used for the courier-facing "my deliveries" view); null returns
    /// everything, as before.</summary>
    Task<IReadOnlyList<OrderListItemDto>> GetAllAsync(int? courierId = null, CancellationToken cancellationToken = default);
    Task<OrderDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Assigns (or reassigns) the courier responsible for delivering an order.</summary>
    Task<AssignCourierResult> AssignCourierAsync(int orderId, int courierId, CancellationToken cancellationToken = default);

    /// <summary>Replaces an out-of-stock line item's product with an alternative — sets
    /// ProductId/Barcode to the substitute product's, flags IsSubstitution, and remembers the
    /// originally-ordered barcode in SubstitutedForBarcode. Purely local: no Trendyol Go
    /// substitution-reporting endpoint was given in the integration brief.</summary>
    Task<SubstituteOrderItemResult> SubstituteOrderItemAsync(
        int orderId, int itemId, int newProductId, CancellationToken cancellationToken = default);
}
