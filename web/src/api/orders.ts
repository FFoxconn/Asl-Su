import { apiFetch } from './client';
import type { OrderDetail, OrderListItem, OrderWorkflowActionResult } from '../types/order';

export const getOrders = () => apiFetch<OrderListItem[]>('/api/orders');

export const getOrder = (id: number) => apiFetch<OrderDetail>(`/api/orders/${id}`);

const postWorkflowAction = (id: number, action: string) =>
  apiFetch<OrderWorkflowActionResult>(`/api/orders/${id}/${action}`, { method: 'POST' });

export const acceptOrder = (id: number) => postWorkflowAction(id, 'accept');
export const startPreparingOrder = (id: number) => postWorkflowAction(id, 'start-preparing');
export const markOrderPrepared = (id: number) => postWorkflowAction(id, 'mark-prepared');
export const deliverOrder = (id: number) => postWorkflowAction(id, 'deliver');
