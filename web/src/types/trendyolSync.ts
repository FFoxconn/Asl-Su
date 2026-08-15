export interface ProductSyncSummary {
  totalProducts: number;
  batchCount: number;
  submittedCount: number;
  failedCount: number;
  batchRequestIds: string[];
  message: string | null;
}
