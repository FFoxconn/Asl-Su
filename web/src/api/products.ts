import { apiFetch } from './client';
import type {
  CreateProductRequest,
  Product,
  SetSaleStatusRequest,
  StockPrice,
  UpsertStockPriceRequest,
} from '../types/product';

export const getProducts = () => apiFetch<Product[]>('/api/products');

export const createProduct = (request: CreateProductRequest) =>
  apiFetch<Product>('/api/products', { method: 'POST', body: JSON.stringify(request) });

export const deleteProduct = (id: number) =>
  apiFetch<void>(`/api/products/${id}`, { method: 'DELETE' });

export const getStockPrice = (productId: number) =>
  apiFetch<StockPrice[]>(`/api/stock-price?productId=${productId}`);

export const upsertStockPrice = (request: UpsertStockPriceRequest) =>
  apiFetch<StockPrice>('/api/stock-price', { method: 'PUT', body: JSON.stringify(request) });

export const setSaleStatus = (request: SetSaleStatusRequest) =>
  apiFetch<StockPrice>('/api/stock-price/sale-status', { method: 'PUT', body: JSON.stringify(request) });
