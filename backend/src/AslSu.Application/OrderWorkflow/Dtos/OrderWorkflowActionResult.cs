namespace AslSu.Application.OrderWorkflow.Dtos;

public enum OrderWorkflowError
{
    NotFound,
    InvalidTransition,
    Forbidden,
}

public record OrderWorkflowActionResult(
    bool Success,
    string? WorkflowStatus,
    bool TrendyolNotified,
    string? TrendyolMessage,
    OrderWorkflowError? Error)
{
    public static OrderWorkflowActionResult Ok(string workflowStatus, bool trendyolNotified, string? trendyolMessage) =>
        new(true, workflowStatus, trendyolNotified, trendyolMessage, null);

    public static OrderWorkflowActionResult Fail(OrderWorkflowError error) =>
        new(false, null, false, null, error);
}
