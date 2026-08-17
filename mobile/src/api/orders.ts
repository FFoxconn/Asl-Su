import { apiFetch } from './client';
import type { OrderListItem } from '../types/order';

export function getMyOrders(): Promise<OrderListItem[]> {
  return apiFetch<OrderListItem[]>('/api/orders/my');
}

export interface DeliverResult {
  success: boolean;
  workflowStatus: string | null;
  trendyolNotified: boolean;
  trendyolMessage: string | null;
}

export function deliverOrder(orderId: number): Promise<DeliverResult> {
  return apiFetch<DeliverResult>(`/api/orders/${orderId}/deliver`, { method: 'POST' });
}
