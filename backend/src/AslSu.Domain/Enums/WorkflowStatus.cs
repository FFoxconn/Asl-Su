namespace AslSu.Domain.Enums;

/// <summary>Local panel fulfilment workflow: Yeni &#8594; Kabul Edildi &#8594; Hazirlaniyor &#8594; Hazirlandi &#8594; Teslim Edildi.</summary>
public enum WorkflowStatus
{
    New = 0,
    Accepted = 1,
    Preparing = 2,
    Prepared = 3,
    Delivered = 4,
}
