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

export const assignCourier = (orderId: number, courierId: number) =>
  apiFetch<OrderDetail>(`/api/orders/${orderId}/courier`, {
    method: 'PUT',
    body: JSON.stringify({ courierId }),
  });

export const substituteOrderItem = (orderId: number, itemId: number, productId: number) =>
  apiFetch<OrderDetail>(`/api/orders/${orderId}/items/${itemId}/substitute`, {
    method: 'PUT',
    body: JSON.stringify({ productId }),
  });
