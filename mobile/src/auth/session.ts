import * as SecureStore from 'expo-secure-store';
import type { AuthResponse } from '../types/auth';

const STORAGE_KEY = 'aslsu.session';

export async function getSession(): Promise<AuthResponse | null> {
  const raw = await SecureStore.getItemAsync(STORAGE_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as AuthResponse;
  } catch {
    return null;
  }
}

export async function setSession(session: AuthResponse): Promise<void> {
  await SecureStore.setItemAsync(STORAGE_KEY, JSON.stringify(session));
}

export async function clearSession(): Promise<void> {
  await SecureStore.deleteItemAsync(STORAGE_KEY);
}
