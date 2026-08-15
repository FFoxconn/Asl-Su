export interface TrendyolSettings {
  supplierId: string;
  maskedApiKey: string;
  isConfigured: boolean;
}

export interface TestConnectionResponse {
  success: boolean;
  message: string;
}
