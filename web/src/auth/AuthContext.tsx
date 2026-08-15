import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import { login as apiLogin } from '../api/auth';
import { clearSession, getSession, setSession } from './session';
import type { AuthResponse } from '../types/auth';

interface AuthContextValue {
  session: AuthResponse | null;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSessionState] = useState<AuthResponse | null>(() => getSession());

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      isAuthenticated: session !== null,
      login: async (email, password) => {
        const result = await apiLogin(email, password);
        setSession(result);
        setSessionState(result);
      },
      logout: () => {
        clearSession();
        setSessionState(null);
      },
    }),
    [session],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
