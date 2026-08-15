namespace AslSu.Domain.Entities;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int? ProductId { get; set; }

    public string Barcode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public bool IsSubstitution { get; set; }
    public string? SubstitutedForBarcode { get; set; }
}
