import { apiFetch } from './client';
import type { Courier } from '../types/courier';

export const getCouriers = () => apiFetch<Courier[]>('/api/couriers');

export const createCourier = (name: string, phone: string | null) =>
  apiFetch<Courier>('/api/couriers', { method: 'POST', body: JSON.stringify({ name, phone }) });
