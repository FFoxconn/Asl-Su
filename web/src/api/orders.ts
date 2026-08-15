import { apiFetch } from './client';
import type { OrderDetail, OrderListItem } from '../types/order';

export const getOrders = () => apiFetch<OrderListItem[]>('/api/orders');

export const getOrder = (id: number) => apiFetch<OrderDetail>(`/api/orders/${id}`);
