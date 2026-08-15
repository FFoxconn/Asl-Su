import { apiFetch } from './client';
import type {
  BatchPollSummary,
  OrderSyncSummary,
  ProductSyncSummary,
  SaleStatusSyncSummary,
  StockPriceSyncSummary,
} from '../types/trendyolSync';

export const pushProducts = () =>
  apiFetch<ProductSyncSummary>('/api/trendyol-sync/products/push', { method: 'POST' });

export const pushStockPrice = () =>
  apiFetch<StockPriceSyncSummary>('/api/trendyol-sync/stock-price/push', { method: 'POST' });

export const pushSaleStatus = () =>
  apiFetch<SaleStatusSyncSummary>('/api/trendyol-sync/sale-status/push', { method: 'POST' });

export const pollBatchRequests = () =>
  apiFetch<BatchPollSummary>('/api/trendyol-sync/batch-requests/poll', { method: 'POST' });

export const pullOrders = () =>
  apiFetch<OrderSyncSummary>('/api/trendyol-sync/orders/pull', { method: 'POST' });
