import { apiFetch } from './client';
import type { BatchPollSummary, ProductSyncSummary, StockPriceSyncSummary } from '../types/trendyolSync';

export const pushProducts = () =>
  apiFetch<ProductSyncSummary>('/api/trendyol-sync/products/push', { method: 'POST' });

export const pushStockPrice = () =>
  apiFetch<StockPriceSyncSummary>('/api/trendyol-sync/stock-price/push', { method: 'POST' });

export const pollBatchRequests = () =>
  apiFetch<BatchPollSummary>('/api/trendyol-sync/batch-requests/poll', { method: 'POST' });
