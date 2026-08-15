using System.Net;
using System.Text.Json;
using AslSu.TrendyolGo.Configuration;
using AslSu.TrendyolGo.Exceptions;
using AslSu.TrendyolGo.Http;
using Microsoft.Extensions.Options;

namespace AslSu.TrendyolGo.Orders;

public class TrendyolOrderClient(HttpClient httpClient, IOptions<TrendyolGoOptions> options) : ITrendyolOrderClient
{
    public Task<HttpResponseMessage> GetPackagesRawAsync(CancellationToken cancellationToken = default)
    {
        var supplierId = options.Value.SupplierId;
        // Endpoint path given in the integration brief; exact query parameters are
        // TODO: confirm against developers.tgoapps.com before relying on this for real order sync.
        return httpClient.GetAsync($"/integrator/order/grocery/suppliers/{supplierId}/packages", cancellationToken);
    }

    public async Task<TrendyolPackagesOutcome> GetPackagesAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Value.IsConfigured)
        {
            return TrendyolPackagesOutcome.Fail("Trendyol Go bağlantı bilgileri henüz yapılandırılmamış.");
        }

        try
        {
            var response = await GetPackagesRawAsync(cancellationToken);
            await TrendyolResponseHandler.EnsureSuccessAsync(response, cancellationToken);
            return await ParseAsync(response, cancellationToken);
        }
        catch (TrendyolAuthException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return TrendyolPackagesOutcome.Fail(TrendyolErrorMessages.CheckCredentials);
        }
        catch (TrendyolAuthException)
        {
            return TrendyolPackagesOutcome.Fail(TrendyolErrorMessages.CheckHeaders);
        }
        catch (TrendyolRateLimitException)
        {
            return TrendyolPackagesOutcome.Fail(TrendyolErrorMessages.RateLimited);
        }
        catch (TrendyolApiException ex) when ((int)ex.StatusCode >= 500)
        {
            return TrendyolPackagesOutcome.Fail(TrendyolErrorMessages.ServiceError);
        }
        catch (TrendyolApiException ex)
        {
            return TrendyolPackagesOutcome.Fail($"Trendyol Go isteği başarısız oldu ({(int)ex.StatusCode}).");
        }
        catch (HttpRequestException)
        {
            return TrendyolPackagesOutcome.Fail(TrendyolErrorMessages.ConnectionFailed);
        }
    }

    /// <summary>Best-effort parse of the packages response. The root may be a bare array, or an
    /// object wrapping the array under a common paging key ("content"/"packages"/"orders") —
    /// TODO: confirm the real response envelope and field names against developers.tgoapps.com.
    /// Each package's original JSON is preserved in <see cref="TrendyolPackageDto.RawJson"/>
    /// regardless of how well the named fields below parsed, so a schema mismatch never loses
    /// data — it just means the mapped fields may be empty until the schema is confirmed.</summary>
    private static async Task<TrendyolPackagesOutcome> ParseAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            var root = document.RootElement;

            var items = root.ValueKind switch
            {
                JsonValueKind.Array => root,
                JsonValueKind.Object when TryGetArrayProperty(root, out var arr) => arr,
                _ => (JsonElement?)null,
            };

            if (items is not { } arrayElement)
            {
                return new TrendyolPackagesOutcome(true, [], null);
            }

            var packages = arrayElement.EnumerateArray().Select(ParsePackage).ToList();
            return new TrendyolPackagesOutcome(true, packages, null);
        }
        catch (JsonException)
        {
            return new TrendyolPackagesOutcome(true, [], null);
        }
    }

    private static bool TryGetArrayProperty(JsonElement root, out JsonElement array)
    {
        foreach (var key in new[] { "content", "packages", "orders", "items", "data" })
        {
            if (root.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.Array)
            {
                array = el;
                return true;
            }
        }

        array = default;
        return false;
    }

    private static TrendyolPackageDto ParsePackage(JsonElement el)
    {
        var packageId = GetString(el, "id", "packageId", "orderId") ?? string.Empty;
        var orderNumber = GetString(el, "orderNumber", "orderId", "id") ?? packageId;
        var status = GetString(el, "status", "packageStatus", "shipmentPackageStatus");
        var orderDate = GetDate(el, "orderDate", "packageDate", "createdDate");
        var invoiceAmount = GetDecimal(el, "totalPrice", "invoiceAmount", "grossAmount");
        var invoiceTaxAmount = GetDecimal(el, "totalTax", "invoiceTaxAmount", "taxAmount");
        var bagCount = GetInt(el, "bagCount", "packageCount");
        var receiptLink = GetString(el, "receiptLink", "invoiceLink");
        var storeTgoId = GetString(el, "storeId", "warehouseId", "supplierAddressId");

        string? customerName = GetString(el, "customerFirstName") is { } first
            ? $"{first} {GetString(el, "customerLastName")}".Trim()
            : GetString(el, "customerName");
        var customerPhone = GetString(el, "customerPhone", "phoneNumber") ?? GetNested(el, "shipmentAddress", "phone");
        var customerAddress = GetNested(el, "shipmentAddress", "address")
            ?? GetNested(el, "invoiceAddress", "address")
            ?? GetString(el, "customerAddress");

        var lines = el.TryGetProperty("lines", out var linesEl) && linesEl.ValueKind == JsonValueKind.Array
            ? linesEl.EnumerateArray().Select(ParseLine).ToList()
            : el.TryGetProperty("items", out var itemsEl) && itemsEl.ValueKind == JsonValueKind.Array
                ? itemsEl.EnumerateArray().Select(ParseLine).ToList()
                : [];

        return new TrendyolPackageDto(
            packageId,
            orderNumber,
            status,
            orderDate,
            string.IsNullOrWhiteSpace(customerName) ? null : customerName,
            customerPhone,
            customerAddress,
            invoiceAmount,
            invoiceTaxAmount,
            bagCount,
            receiptLink,
            storeTgoId,
            lines,
            el.GetRawText());
    }

    private static TrendyolPackageLineDto ParseLine(JsonElement el)
    {
        var barcode = GetString(el, "barcode", "productBarcode") ?? string.Empty;
        var quantity = GetInt(el, "quantity", "amount") ?? 0;
        var unitPrice = GetDecimal(el, "price", "unitPrice", "salePrice") ?? 0m;
        var substitutedFor = GetString(el, "substitutedForBarcode", "originalBarcode");

        return new TrendyolPackageLineDto(barcode, quantity, unitPrice, substitutedFor is not null, substitutedFor);
    }

    private static string? GetString(JsonElement el, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (el.TryGetProperty(key, out var value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => value.GetString(),
                    JsonValueKind.Number => value.ToString(),
                    _ => null,
                };
            }
        }

        return null;
    }

    private static string? GetNested(JsonElement el, string objectKey, string fieldKey) =>
        el.TryGetProperty(objectKey, out var nested) && nested.ValueKind == JsonValueKind.Object
            ? GetString(nested, fieldKey)
            : null;

    private static int? GetInt(JsonElement el, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (el.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Number
                && value.TryGetInt32(out var i))
            {
                return i;
            }
        }

        return null;
    }

    private static decimal? GetDecimal(JsonElement el, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (el.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Number
                && value.TryGetDecimal(out var d))
            {
                return d;
            }
        }

        return null;
    }

    /// <summary>Trendyol's marketplace APIs commonly use epoch-millisecond timestamps —
    /// TODO: confirm against developers.tgoapps.com. Falls back to ISO-8601 string parsing.</summary>
    private static DateTime? GetDate(JsonElement el, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (!el.TryGetProperty(key, out var value))
            {
                continue;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var epochMs))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(epochMs).UtcDateTime;
            }

            if (value.ValueKind == JsonValueKind.String && DateTime.TryParse(
                    value.GetString(), null,
                    System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
                    out var parsed))
            {
                return parsed;
            }
        }

        return null;
    }
}
