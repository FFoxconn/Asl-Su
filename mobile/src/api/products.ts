import { apiFetch } from './client';
import type { Product } from '../types/product';

export const getProducts = () => apiFetch<Product[]>('/api/products');
