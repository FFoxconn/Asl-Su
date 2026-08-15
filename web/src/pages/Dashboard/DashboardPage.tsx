import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../../auth/AuthContext';
import { getHealth, type HealthStatus } from '../../api/auth';
import { ApiError } from '../../api/client';

export function DashboardPage() {
  const { session, logout } = useAuth();
  const [health, setHealth] = useState<HealthStatus | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getHealth()
      .then(setHealth)
      .catch((err) => setError(err instanceof ApiError ? err.message : 'Backend\'e ulaşılamadı.'));
  }, []);

  return (
    <div style={{ maxWidth: 640, margin: '2rem auto', fontFamily: 'system-ui' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Genel Bakış</h1>
        <button onClick={logout}>Çıkış Yap</button>
      </header>
      <p>
        Hoş geldin, <strong>{session?.displayName}</strong> ({session?.role})
      </p>
      <section>
        <h2>Backend Durumu</h2>
        {error && <p style={{ color: 'crimson' }}>{error}</p>}
        {health && (
          <p>
            Durum: <strong>{health.status}</strong> — {new Date(health.timeUtc).toLocaleString()}
          </p>
        )}
        {!health && !error && <p>Kontrol ediliyor...</p>}
      </section>
      <nav style={{ display: 'flex', gap: 16 }}>
        <Link to="/products">Ürünler →</Link>
        <Link to="/api-settings">API Ayarları →</Link>
      </nav>
    </div>
  );
}
