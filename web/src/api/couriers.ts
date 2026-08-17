import { apiFetch } from './client';
import type { Courier, CourierStats } from '../types/courier';

export const getCouriers = () => apiFetch<Courier[]>('/api/couriers');

export const createCourier = (name: string, phone: string | null, email: string | null, password: string | null) =>
  apiFetch<Courier>('/api/couriers', { method: 'POST', body: JSON.stringify({ name, phone, email, password }) });

export const getCourierStats = (id: number) => apiFetch<CourierStats>(`/api/couriers/${id}/stats`);

export const setCourierActive = (id: number, isActive: boolean) =>
  apiFetch<Courier>(`/api/couriers/${id}/status`, { method: 'PUT', body: JSON.stringify({ isActive }) });

export const setCourierLogin = (id: number, email: string, password: string) =>
  apiFetch<Courier>(`/api/couriers/${id}/login`, { method: 'PUT', body: JSON.stringify({ email, password }) });