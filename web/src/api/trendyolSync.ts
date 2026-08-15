import { apiFetch } from './client';
import type { ProductSyncSummary } from '../types/trendyolSync';

export const pushProducts = () =>
  apiFetch<ProductSyncSummary>('/api/trendyol-sync/products/push', { method: 'POST' });
