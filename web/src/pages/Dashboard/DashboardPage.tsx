import { useEffect, useState, type ReactNode } from 'react';
import { Link as RouterLink } from 'react-router-dom';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import LinearProgress from '@mui/material/LinearProgress';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import ReceiptLongOutlinedIcon from '@mui/icons-material/ReceiptLongOutlined';
import SettingsOutlinedIcon from '@mui/icons-material/SettingsOutlined';
import LocalShippingOutlinedIcon from '@mui/icons-material/LocalShippingOutlined';
import PaymentsOutlinedIcon from '@mui/icons-material/PaymentsOutlined';
import { useAuth } from '../../auth/AuthContext';
import { getHealth, type HealthStatus } from '../../api/auth';
import { getOrders } from '../../api/orders';
import { getCouriers } from '../../api/couriers';
import { ApiError } from '../../api/client';
import type { OrderListItem } from '../../types/order';
import type { Courier } from '../../types/courier';

const WORKFLOW_STEPS: { key: string; label: string; color: string }[] = [
  { key: 'New', label: 'Yeni', color: '#8a94a6' },
  { key: 'Accepted', label: 'Kabul Edildi', color: '#0b6e99' },
  { key: 'Preparing', label: 'Hazırlanıyor', color: '#b5730c' },
  { key: 'Prepared', label: 'Hazırlandı', color: '#7c4dff' },
  { key: 'Delivered', label: 'Teslim Edildi', color: '#1f9254' },
];

const WORKFLOW_CHIP_COLOR: Record<string, 'default' | 'info' | 'warning' | 'secondary' | 'success'> = {
  New: 'default',
  Accepted: 'info',
  Preparing: 'warning',
  Prepared: 'secondary',
  Delivered: 'success',
};

const QUICK_LINKS = [
  { to: '/products', label: 'Ürünler', description: 'Ürün, stok ve fiyat yönetimi', icon: <Inventory2OutlinedIcon /> },
  { to: '/orders', label: 'Siparişler', description: 'Trendyol Go sipariş akışı', icon: <ReceiptLongOutlinedIcon /> },
  { to: '/couriers', label: 'Kuryeler', description: 'Kurye listesi ve sipariş ataması', icon: <LocalShippingOutlinedIcon /> },
  { to: '/api-settings', label: 'API Ayarları', description: 'Bağlantı durumu ve test', icon: <SettingsOutlinedIcon /> },
];

function isToday(iso: string): boolean {
  const d = new Date(iso);
  const now = new Date();
  return d.getFullYear() === now.getFullYear() && d.getMonth() === now.getMonth() && d.getDate() === now.getDate();
}

function KpiCard({
  icon,
  label,
  value,
  bg,
  color,
}: {
  icon: ReactNode;
  label: string;
  value: string;
  bg: string;
  color: string;
}) {
  return (
    <Box
      sx={{
        flex: '1 1 160px',
        display: 'flex',
        gap: 1.5,
        alignItems: 'center',
        bgcolor: 'background.paper',
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 3,
        p: 2,
      }}
    >
      <Box
        sx={{
          width: 40,
          height: 40,
          borderRadius: 2,
          flexShrink: 0,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          bgcolor: bg,
          color,
        }}
      >
        {icon}
      </Box>
      <Box sx={{ minWidth: 0 }}>
        <Typography variant="body2" color="text.secondary" noWrap sx={{ fontSize: '0.78rem' }}>
          {label}
        </Typography>
        <Typography sx={{ fontWeight: 700, fontSize: '1.3rem' }} noWrap>
          {value}
        </Typography>
      </Box>
    </Box>
  );
}

export function DashboardPage() {
  const { session } = useAuth();
  const [health, setHealth] = useState<HealthStatus | null>(null);
  const [healthError, setHealthError] = useState<string | null>(null);
  const [orders, setOrders] = useState<OrderListItem[] | null>(null);
  const [couriers, setCouriers] = useState<Courier[] | null>(null);

  useEffect(() => {
    getHealth()
      .then(setHealth)
      .catch((err) => setHealthError(err instanceof ApiError ? err.message : "Backend'e ulaşılamadı."));
    getOrders().then(setOrders).catch(() => setOrders([]));
    getCouriers().then(setCouriers).catch(() => setCouriers([]));
  }, []);

  const todayOrders = orders?.filter((o) => isToday(o.orderDate)) ?? [];
  const todayRevenue = todayOrders.reduce((sum, o) => sum + (o.invoiceAmount ?? 0), 0);
  const activeCourierCount = couriers?.filter((c) => c.isActive).length ?? 0;

  const recentOrders = orders != null
    ? [...orders].sort((a, b) => new Date(b.orderDate).getTime() - new Date(a.orderDate).getTime()).slice(0, 8)
    : [];

  const statusCounts = (orders ?? []).reduce<Record<string, number>>((acc, o) => {
    acc[o.workflowStatus] = (acc[o.workflowStatus] ?? 0) + 1;
    return acc;
  }, {});
  const totalForBreakdown = orders?.length ?? 0;

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

      <Stack direction="row" flexWrap="wrap" gap={2} sx={{ mb: 4 }}>
        <KpiCard
          icon={<ReceiptLongOutlinedIcon />}
          label="Toplam Sipariş"
          value={orders != null ? `${orders.length}` : '-'}
          bg="#e5f0fd"
          color="#0b6e99"
        />
        <KpiCard
          icon={<Inventory2OutlinedIcon />}
          label="Bugünkü Sipariş"
          value={orders != null ? `${todayOrders.length}` : '-'}
          bg="#e5f0fd"
          color="#0b6e99"
        />
        <KpiCard
          icon={<PaymentsOutlinedIcon />}
          label="Bugünkü Ciro"
          value={orders != null ? `${todayRevenue.toFixed(2)} ₺` : '-'}
          bg="#dcf3e4"
          color="#1f9254"
        />
        <KpiCard
          icon={<LocalShippingOutlinedIcon />}
          label="Aktif Kurye"
          value={couriers != null ? `${activeCourierCount}` : '-'}
          bg="#fdecc8"
          color="#b5730c"
        />
      </Stack>

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
            bgcolor: healthError ? '#fbe7e7' : health ? '#dcf3e4' : 'action.hover',
            color: healthError ? 'error.main' : health ? 'success.main' : 'text.secondary',
          }}
        >
          {healthError ? <ErrorOutlineIcon /> : health ? <CheckCircleOutlineIcon /> : <HourglassEmptyIcon />}
        </Box>
        <Box sx={{ minWidth: 0 }}>
          <Typography variant="body2" color="text.secondary">
            Backend Durumu
          </Typography>
          {healthError && (
            <Typography sx={{ color: 'error.main', fontWeight: 600, fontSize: '0.95rem' }}>{healthError}</Typography>
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
          {!health && !healthError && <Typography sx={{ fontWeight: 600 }}>Kontrol ediliyor...</Typography>}
        </Box>
      </Box>

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: { xs: '1fr', md: '2fr 1fr' },
          gap: 2,
          mb: 4,
          alignItems: 'start',
        }}
      >
        <Box
          sx={{
            bgcolor: 'background.paper',
            border: '1px solid',
            borderColor: 'divider',
            borderRadius: 3,
            overflow: 'hidden',
          }}
        >
          <Typography variant="h2" sx={{ p: 2, pb: 1.5 }}>
            Son Siparişler
          </Typography>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Sipariş No</TableCell>
                  <TableCell>Müşteri</TableCell>
                  <TableCell>Durum</TableCell>
                  <TableCell>Kurye</TableCell>
                  <TableCell align="right">Tutar</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {recentOrders.map((o) => (
                  <TableRow key={o.id} hover>
                    <TableCell>{o.orderNumber}</TableCell>
                    <TableCell>{o.customerName ?? '-'}</TableCell>
                    <TableCell>
                      <Chip
                        size="small"
                        label={WORKFLOW_STEPS.find((s) => s.key === o.workflowStatus)?.label ?? o.workflowStatus}
                        color={WORKFLOW_CHIP_COLOR[o.workflowStatus] ?? 'default'}
                      />
                    </TableCell>
                    <TableCell>{o.courierName ?? '-'}</TableCell>
                    <TableCell align="right">
                      {o.invoiceAmount != null ? `${o.invoiceAmount.toFixed(2)} ₺` : '-'}
                    </TableCell>
                  </TableRow>
                ))}
                {orders != null && recentOrders.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5}>
                      <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                        Henüz sipariş yok.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>

        <Box
          sx={{
            bgcolor: 'background.paper',
            border: '1px solid',
            borderColor: 'divider',
            borderRadius: 3,
            p: 2.5,
          }}
        >
          <Typography variant="h2" sx={{ mb: 2 }}>
            Sipariş Durumu Dağılımı
          </Typography>
          <Stack spacing={2}>
            {WORKFLOW_STEPS.map((step) => {
              const count = statusCounts[step.key] ?? 0;
              const pct = totalForBreakdown > 0 ? (count / totalForBreakdown) * 100 : 0;
              return (
                <Box key={step.key}>
                  <Stack direction="row" justifyContent="space-between" sx={{ mb: 0.5 }}>
                    <Typography variant="body2">{step.label}</Typography>
                    <Typography variant="body2" sx={{ fontWeight: 700 }}>
                      {count}
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={pct}
                    sx={{
                      height: 6,
                      borderRadius: 3,
                      bgcolor: 'action.hover',
                      '& .MuiLinearProgress-bar': { bgcolor: step.color, borderRadius: 3 },
                    }}
                  />
                </Box>
              );
            })}
            {totalForBreakdown === 0 && (
              <Typography variant="body2" color="text.secondary">
                Henüz sipariş yok.
              </Typography>
            )}
          </Stack>
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
