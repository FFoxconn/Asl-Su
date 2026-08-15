namespace AslSu.Application.TrendyolSettings.Dtos;

public record TrendyolSettingsDto(string SupplierId, string MaskedApiKey, bool IsConfigured);

public record TestConnectionResponse(bool Success, string Message);
