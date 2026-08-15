import { useEffect, useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Card from '@mui/material/Card';
import CardActionArea from '@mui/material/CardActionArea';
import CardContent from '@mui/material/CardContent';
import Chip from '@mui/material/Chip';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import Typography from '@mui/material/Typography';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import { useAuth } from '../../auth/AuthContext';
import { getHealth, type HealthStatus } from '../../api/auth';
import { ApiError } from '../../api/client';

const QUICK_LINKS = [
  { to: '/products', label: 'Ürünler', description: 'Ürün, stok ve fiyat yönetimi', icon: <Inventory2OutlinedIcon fontSize="large" /> },
  { to: '/orders', label: 'Siparişler', description: 'Trendyol Go sipariş akışı', icon: <ReceiptLongOutlinedIcon fontSize="large" /> },
  { to: '/api-settings', label: 'API Ayarları', description: 'Bağlantı durumu ve test', icon: <SettingsOutlinedIcon fontSize="large" /> },
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
      <Typography variant="h1" gutterBottom>
        Genel Bakış
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Hoş geldin, <strong>{session?.displayName}</strong> ({session?.role})
      </Typography>

      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h2" gutterBottom>
            Backend Durumu
          </Typography>
          {error && <Alert severity="error">{error}</Alert>}
          {health && (
            <Stack direction="row" spacing={1} alignItems="center">
              <Chip icon={<CheckCircleOutlineIcon />} label={health.status} color="success" size="small" />
              <Typography variant="body2" color="text.secondary">
                {new Date(health.timeUtc).toLocaleString()}
              </Typography>
            </Stack>
          )}
          {!health && !error && (
            <Typography variant="body2" color="text.secondary">
              Kontrol ediliyor...
            </Typography>
          )}
        </CardContent>
      </Card>

      <Grid container spacing={2}>
        {QUICK_LINKS.map((link) => (
          <Grid key={link.to} item xs={12} sm={4}>
            <Card variant="outlined">
              <CardActionArea component={RouterLink} to={link.to} sx={{ height: '100%', p: 1 }}>
                <CardContent>
                  <Stack spacing={1} alignItems="flex-start">
                    <Box sx={{ color: 'primary.main' }}>{link.icon}</Box>
                    <Typography variant="h2">{link.label}</Typography>
                    <Typography variant="body2" color="text.secondary">
                      {link.description}
                    </Typography>
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Box>
  );
}
