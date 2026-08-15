import { useEffect, useState } from 'react';
import { getTrendyolSettings, testTrendyolConnection } from '../../api/trendyolSettings';
import type { TrendyolSettings } from '../../types/trendyolSettings';
import { ApiError } from '../../api/client';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Divider from '@mui/material/Divider';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import WifiTetheringIcon from '@mui/icons-material/WifiTethering';

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
    <Box sx={{ maxWidth: 640 }}>
      <Typography variant="h1" gutterBottom>
        API Ayarları
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
        Trendyol Go Supplier ID, API Key ve API Secret bilgileri yalnızca sunucu tarafında güvenli
        yapılandırma (user-secrets / ortam değişkenleri) üzerinden ayarlanır — bu ekrandan değiştirilemez.
      </Typography>

      {loadError && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {loadError}
        </Alert>
      )}

      {settings && (
        <Paper variant="outlined" sx={{ p: 3, mb: 3 }}>
          <Stack spacing={2}>
            <Stack direction="row" justifyContent="space-between">
              <Typography variant="body2" color="text.secondary">
                Supplier ID
              </Typography>
              <Typography variant="body2" fontWeight={600}>
                {settings.supplierId || '—'}
              </Typography>
            </Stack>
            <Divider />
            <Stack direction="row" justifyContent="space-between">
              <Typography variant="body2" color="text.secondary">
                API Key
              </Typography>
              <Typography variant="body2" fontWeight={600}>
                {settings.maskedApiKey || '—'}
              </Typography>
            </Stack>
            <Divider />
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="body2" color="text.secondary">
                Durum
              </Typography>
              <Chip
                size="small"
                color={settings.isConfigured ? 'success' : 'error'}
                label={settings.isConfigured ? 'Yapılandırılmış' : 'Yapılandırılmamış'}
              />
            </Stack>
          </Stack>
        </Paper>
      )}

      <Button
        variant="contained"
        startIcon={isTesting ? <CircularProgress size={16} color="inherit" /> : <WifiTetheringIcon />}
        onClick={handleTestConnection}
        disabled={isTesting}
      >
        {isTesting ? 'Test ediliyor...' : 'API Bağlantısını Test Et'}
      </Button>

      {testResult && (
        <Alert severity={testResult.success ? 'success' : 'error'} sx={{ mt: 2 }}>
          {testResult.message}
        </Alert>
      )}
    </Box>
  );
}
