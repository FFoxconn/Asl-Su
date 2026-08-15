export interface ProductSyncSummary {
  totalProducts: number;
  batchCount: number;
  submittedCount: number;
  failedCount: number;
  batchRequestIds: string[];
  message: string | null;
}

export interface StockPriceSyncSummary {
  totalItems: number;
  batchCount: number;
  submittedCount: number;
  failedCount: number;
  batchRequestIds: string[];
  message: string | null;
}

export interface BatchPollSummary {
  polledCount: number;
  completedCount: number;
  stillProcessingCount: number;
  failedCount: number;
  message: string | null;
}

export interface SaleStatusSyncSummary {
  totalItems: number;
  batchCount: number;
  submittedCount: number;
  failedCount: number;
  batchRequestIds: string[];
  message: string | null;
}

export interface OrderSyncSummary {
  totalFetched: number;
  newCount: number;
  updatedCount: number;
  skippedCount: number;
  message: string | null;
}
