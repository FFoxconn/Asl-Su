import { clearSession, getSession } from '../auth/session';

const BASE_URL = process.env.EXPO_PUBLIC_API_BASE_URL ?? 'https://localhost:5001';

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

export async function apiFetch<T>(path: string, options: RequestInit = {}): Promise<T> {
  const session = await getSession();
  const headers = new Headers(options.headers);
  headers.set('Content-Type', 'application/json');
  if (session?.accessToken) {
    headers.set('Authorization', `Bearer ${session.accessToken}`);
  }

  const response = await fetch(`${BASE_URL}${path}`, { ...options, headers });

  if (response.status === 401) {
    await clearSession();
    throw new ApiError(401, 'Oturum süresi doldu, lütfen tekrar giriş yapın.');
  }

  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new ApiError(response.status, body?.title ?? body?.message ?? 'İstek başarısız oldu.');
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
