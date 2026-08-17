import { useEffect, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import Box from '@mui/material/Box';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import LocalShippingOutlinedIcon from '@mui/icons-material/LocalShippingOutlined';
import { useAuth } from '../../auth/AuthContext';
import { getHealth, type HealthStatus } from '../../api/auth';
import { ApiError } from '../../api/client';

const QUICK_LINKS = [
  { to: '/products', label: 'Ürünler', description: 'Ürün, stok ve fiyat yönetimi', icon: <Inventory2OutlinedIcon /> },
  { to: '/orders', label: 'Siparişler', description: 'Trendyol Go sipariş akışı', icon: <ReceiptLongOutlinedIcon /> },
  { to: '/couriers', label: 'Kuryeler', description: 'Kurye listesi ve sipariş ataması', icon: <LocalShippingOutlinedIcon /> },
  { to: '/api-settings', label: 'API Ayarları', description: 'Bağlantı durumu ve test', icon: <SettingsOutlinedIcon /> },
];

export function DashboardPage() {
  const { session } = useAuth();
  const [health, setHealth] = useState<HealthStatus | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getHealth()
      .then(setHealth)
      .catch((err) => setError(err instanceof ApiError ? err.message : "Backend'e ulaşılamadı."));
  }, []);

  return (
    <Box>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h1" gutterBottom>
          Genel Bakış
        </Typography>
        <Typography variant="body1" color="text.secondary">
          Hoş geldin,{' '}
          <Box component="strong" sx={{ color: 'text.primary' }}>
            {session?.displayName}
          </Box>{' '}
          · {session?.role}
        </Typography>
      </Box>

      <Box
        sx={{
          display: 'flex',
          alignItems: 'center',
          gap: 2,
          bgcolor: 'background.paper',
          border: '1px solid',
          borderColor: 'divider',
          borderRadius: 3,
          p: 2.5,
          mb: 4,
        }}
      >
        <Box
          sx={{
            width: 44,
            height: 44,
            borderRadius: 2.5,
            flexShrink: 0,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            bgcolor: error ? '#fbe7e7' : health ? '#dcf3e4' : 'action.hover',
            color: error ? 'error.main' : health ? 'success.main' : 'text.secondary',
          }}
        >
          {error ? <ErrorOutlineIcon /> : health ? <CheckCircleOutlineIcon /> : <HourglassEmptyIcon />}
        </Box>
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="body2" color="text.secondary">
            Backend Durumu
          </Typography>
          {error && (
            <Typography sx={{ color: 'error.main', fontWeight: 600, fontSize: '0.95rem' }}>{error}</Typography>
          )}
          {health && (
            <Stack direction="row" spacing={1} alignItems="baseline">
              <Typography sx={{ fontWeight: 700, fontSize: '1.1rem', textTransform: 'uppercase' }}>
                {health.status}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {new Date(health.timeUtc).toLocaleString('tr-TR')}
              </Typography>
            </Stack>
          )}
          {!health && !error && <Typography sx={{ fontWeight: 600 }}>Kontrol ediliyor...</Typography>}
        </Box>
      </Box>

      <Typography variant="h2" sx={{ mb: 2 }}>
        Hızlı İşlemler
      </Typography>
      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)', md: 'repeat(4, 1fr)' },
          gap: 2,
        }}
      >
        {QUICK_LINKS.map((link) => (
          <Box
            key={link.to}
            component={RouterLink}
            to={link.to}
            sx={{
              display: 'flex',
              flexDirection: 'column',
              gap: 1.5,
              p: 2.5,
              bgcolor: 'background.paper',
              border: '1px solid',
              borderColor: 'divider',
              borderRadius: 3,
              textDecoration: 'none',
              color: 'text.primary',
              transition: 'border-color .15s ease, box-shadow .15s ease, transform .15s ease',
              '&:hover': {
                borderColor: 'primary.main',
                boxShadow: '0 8px 24px rgba(11,110,153,0.12)',
                transform: 'translateY(-2px)',
              },
            }}
          >
            <Box
              sx={{
                width: 40,
                height: 40,
                borderRadius: 2,
                bgcolor: '#e5f0fd',
                color: 'primary.main',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              {link.icon}
            </Box>
            <Box>
              <Typography sx={{ fontWeight: 700, fontSize: '0.95rem' }}>{link.label}</Typography>
              <Typography variant="body2" color="text.secondary" sx={{ mt: 0.25 }}>
                {link.description}
              </Typography>
            </Box>
          </Box>
        ))}
      </Box>
    </Box>
  );
}
