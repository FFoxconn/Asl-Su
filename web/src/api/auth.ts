import { apiFetch } from './client';
import type { AuthResponse } from '../types/auth';

export function login(email: string, password: string): Promise<AuthResponse> {
  return apiFetch<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  });
}

export interface HealthStatus {
  status: string;
  timeUtc: string;
}

export function getHealth(): Promise<HealthStatus> {
  return apiFetch<HealthStatus>('/api/health');
}
