import { apiFetch } from './client';
import type { Brand, Category, Store } from '../types/catalog';

export const getStores = () => apiFetch<Store[]>('/api/stores');
export const createStore = (name: string, code: string) =>
  apiFetch<Store>('/api/stores', { method: 'POST', body: JSON.stringify({ name, code, address: null }) });

export const getCategories = () => apiFetch<Category[]>('/api/categories');
export const createCategory = (name: string) =>
  apiFetch<Category>('/api/categories', { method: 'POST', body: JSON.stringify({ name, parentCategoryId: null }) });

export const getBrands = () => apiFetch<Brand[]>('/api/brands');
export const createBrand = (name: string) =>
  apiFetch<Brand>('/api/brands', { method: 'POST', body: JSON.stringify({ name }) });
