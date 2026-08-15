import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { getTrendyolSettings, testTrendyolConnection } from '../../api/trendyolSettings';
import type { TrendyolSettings } from '../../types/trendyolSettings';
import { ApiError } from '../../api/client';

export function ApiSettingsPage() {
  const [settings, setSettings] = useState<TrendyolSettings | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);
  const [isTesting, setIsTesting] = useState(false);

  useEffect(() => {
    getTrendyolSettings()
      .then(setSettings)
      .catch((e) => setLoadError(e instanceof ApiError ? e.message : 'Ayarlar yüklenemedi.'));
  }, []);

  async function handleTestConnection() {
    setIsTesting(true);
    setTestResult(null);
    try {
      const result = await testTrendyolConnection();
      setTestResult(result);
    } catch (e) {
      setTestResult({ success: false, message: e instanceof ApiError ? e.message : 'Bağlantı testi başarısız oldu.' });
    } finally {
      setIsTesting(false);
    }
  }

  return (
    <div style={{ maxWidth: 600, margin: '2rem auto', fontFamily: 'system-ui' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>API Ayarları</h1>
        <Link to="/">Genel Bakışa Dön</Link>
      </header>

      <p style={{ color: '#666', fontSize: 14 }}>
        Trendyol Go Supplier ID, API Key ve API Secret bilgileri yalnızca sunucu tarafında güvenli
        yapılandırma (user-secrets / ortam değişkenleri) üzerinden ayarlanır — bu ekrandan değiştirilemez.
      </p>

      {loadError && <p style={{ color: 'crimson' }}>{loadError}</p>}

      {settings && (
        <table style={{ width: '100%', marginBottom: 24 }}>
          <tbody>
            <tr>
              <td style={{ fontWeight: 600, padding: '8px 0' }}>Supplier ID</td>
              <td>{settings.supplierId || '—'}</td>
            </tr>
            <tr>
              <td style={{ fontWeight: 600, padding: '8px 0' }}>API Key</td>
              <td>{settings.maskedApiKey || '—'}</td>
            </tr>
            <tr>
              <td style={{ fontWeight: 600, padding: '8px 0' }}>Durum</td>
              <td style={{ color: settings.isConfigured ? 'green' : 'crimson' }}>
                {settings.isConfigured ? 'Yapılandırılmış' : 'Yapılandırılmamış'}
              </td>
            </tr>
          </tbody>
        </table>
      )}

      <button onClick={handleTestConnection} disabled={isTesting}>
        {isTesting ? 'Test ediliyor...' : 'API Bağlantısını Test Et'}
      </button>

      {testResult && (
        <p style={{ marginTop: 16, color: testResult.success ? 'green' : 'crimson' }}>{testResult.message}</p>
      )}
    </div>
  );
}
