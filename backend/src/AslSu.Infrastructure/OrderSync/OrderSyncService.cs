using AslSu.Application.OrderSync;
using AslSu.Application.OrderSync.Dtos;
using AslSu.Domain.Entities;
using AslSu.Domain.Enums;
using AslSu.Infrastructure.Persistence;
using AslSu.TrendyolGo.Orders;
using Microsoft.EntityFrameworkCore;

namespace AslSu.Infrastructure.OrderSync;

public class OrderSyncService(AslSuDbContext dbContext, ITrendyolOrderClient orderClient) : IOrderSyncService
{
    public async Task<OrderSyncSummary> PullOrdersAsync(CancellationToken cancellationToken = default)
    {
        var outcome = await orderClient.GetPackagesAsync(cancellationToken);
        if (!outcome.Success)
        {
            return new OrderSyncSummary(0, 0, 0, 0, outcome.ErrorMessage);
        }

        if (outcome.Packages.Count == 0)
        {
            return new OrderSyncSummary(0, 0, 0, 0, "Yeni sipariş bulunamadı.");
        }

        var stores = await dbContext.Stores.OrderBy(s => s.Id).ToListAsync(cancellationToken);
        if (stores.Count == 0)
        {
            return new OrderSyncSummary(
                outcome.Packages.Count, 0, 0, outcome.Packages.Count,
                "Sipariş eşleştirmek için önce en az bir şube tanımlanmalı.");
        }

        // Trendyol Go's package payload doesn't confirm a store/warehouse field yet — a
        // package is matched to a store by Store.TgoStoreId if present, otherwise it falls
        // back to the first store. TODO: confirm the real field once multi-store mapping
        // is documented at developers.tgoapps.com.
        var defaultStore = stores[0];

        var packageIds = outcome.Packages.Select(p => p.PackageId).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
        var existingOrders = await dbContext.Orders
            .Where(o => packageIds.Contains(o.PackageId))
            .ToDictionaryAsync(o => o.PackageId, cancellationToken);

        var barcodes = outcome.Packages.SelectMany(p => p.Lines).Select(l => l.Barcode)
            .Where(b => !string.IsNullOrWhiteSpace(b)).Distinct().ToList();
        var productsByBarcode = await dbContext.Products
            .Where(p => barcodes.Contains(p.Barcode))
            .ToDictionaryAsync(p => p.Barcode, cancellationToken);

        var newCount = 0;
        var updatedCount = 0;
        var skippedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var pkg in outcome.Packages)
        {
            if (string.IsNullOrWhiteSpace(pkg.PackageId))
            {
                skippedCount++;
                continue;
            }

            var status = ParseStatus(pkg.Status);

            if (existingOrders.TryGetValue(pkg.PackageId, out var existing))
            {
                existing.OrderNumber = pkg.OrderNumber;
                existing.Status = status;
                existing.OrderDate = pkg.OrderDate ?? existing.OrderDate;
                existing.InvoiceAmount = pkg.InvoiceAmount;
                existing.InvoiceTaxAmount = pkg.InvoiceTaxAmount;
                existing.BagCount = pkg.BagCount;
                existing.ReceiptLink = pkg.ReceiptLink;
                existing.RawPayloadJson = pkg.RawJson;
                existing.UpdatedAt = now;
                // WorkflowStatus and CustomerId are intentionally left untouched on update —
                // the local fulfilment workflow only advances via the Phase 9 status-transition
                // actions, and re-linking a customer on every pull isn't worth the complexity here.

                var existingItems = await dbContext.OrderItems
                    .Where(i => i.OrderId == existing.Id).ToListAsync(cancellationToken);
                dbContext.OrderItems.RemoveRange(existingItems);

                foreach (var line in pkg.Lines)
                {
                    dbContext.OrderItems.Add(BuildOrderItem(existing.Id, line, productsByBarcode));
                }

                await dbContext.SaveChangesAsync(cancellationToken);
                updatedCount++;
                continue;
            }

            int? customerId = null;
            if (!string.IsNullOrWhiteSpace(pkg.CustomerName))
            {
                var customer = new Customer { Name = pkg.CustomerName, Phone = pkg.CustomerPhone, Address = pkg.CustomerAddress };
                dbContext.Customers.Add(customer);
                await dbContext.SaveChangesAsync(cancellationToken);
                customerId = customer.Id;
            }

            var store = (pkg.StoreTgoId is not null
                ? stores.FirstOrDefault(s => s.TgoStoreId == pkg.StoreTgoId)
                : null) ?? defaultStore;

            var order = new Order
            {
                PackageId = pkg.PackageId,
                OrderNumber = pkg.OrderNumber,
                StoreId = store.Id,
                CustomerId = customerId,
                Status = status,
                WorkflowStatus = WorkflowStatus.New,
                OrderDate = pkg.OrderDate ?? now,
                InvoiceAmount = pkg.InvoiceAmount,
                InvoiceTaxAmount = pkg.InvoiceTaxAmount,
                BagCount = pkg.BagCount,
                ReceiptLink = pkg.ReceiptLink,
                RawPayloadJson = pkg.RawJson,
                CreatedAt = now,
                UpdatedAt = now,
            };
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            foreach (var line in pkg.Lines)
            {
                dbContext.OrderItems.Add(BuildOrderItem(order.Id, line, productsByBarcode));
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            newCount++;
        }

        var message = $"{outcome.Packages.Count} sipariş çekildi: {newCount} yeni, {updatedCount} güncellendi" +
            (skippedCount > 0 ? $", {skippedCount} atlandı." : ".");

        return new OrderSyncSummary(outcome.Packages.Count, newCount, updatedCount, skippedCount, message);
    }

    private static OrderItem BuildOrderItem(
        int orderId, TrendyolPackageLineDto line, IReadOnlyDictionary<string, Product> productsByBarcode) => new()
    {
        OrderId = orderId,
        ProductId = productsByBarcode.TryGetValue(line.Barcode, out var product) ? product.Id : null,
        Barcode = line.Barcode,
        Quantity = line.Quantity,
        UnitPrice = line.UnitPrice,
        IsSubstitution = line.IsSubstitution,
        SubstitutedForBarcode = line.SubstitutedForBarcode,
    };

    /// <summary>Best-effort status text match against the Trendyol Go status vocabulary —
    /// falls back to Created for anything unrecognized rather than throwing.</summary>
    private static OrderStatus ParseStatus(string? status) => status?.Trim().ToLowerInvariant() switch
    {
        "created" => OrderStatus.Created,
        "picking" => OrderStatus.Picking,
        "invoiced" => OrderStatus.Invoiced,
        "shipped" => OrderStatus.Shipped,
        "cancelled" or "canceled" => OrderStatus.Cancelled,
        "delivered" => OrderStatus.Delivered,
        "returned" => OrderStatus.Returned,
        "unpacked" => OrderStatus.UnPacked,
        "unsupplied" => OrderStatus.UnSupplied,
        _ => OrderStatus.Created,
    };
}
