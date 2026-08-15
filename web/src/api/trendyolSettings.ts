import { apiFetch } from './client';
import type { TestConnectionResponse, TrendyolSettings } from '../types/trendyolSettings';

export const getTrendyolSettings = () => apiFetch<TrendyolSettings>('/api/trendyol-settings');

export const testTrendyolConnection = () =>
  apiFetch<TestConnectionResponse>('/api/trendyol-settings/test-connection', { method: 'POST' });
