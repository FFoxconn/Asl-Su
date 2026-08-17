import { useEffect, useState, type ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import IconButton from '@mui/material/IconButton';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import CancelOutlinedIcon from '@mui/icons-material/CancelOutlined';
import AssignmentReturnOutlinedIcon from '@mui/icons-material/AssignmentReturnOutlined';
import PaidOutlinedIcon from '@mui/icons-material/PaidOutlined';
import { getCourierStats } from '../../api/couriers';
import type { CourierStats } from '../../types/courier';

const STATUS_COLOR: Record<string, 'default' | 'info' | 'success' | 'error' | 'warning'> = {
  Created: 'default',
  Picking: 'info',
  Invoiced: 'info',
  Shipped: 'info',
  Delivered: 'success',
  Cancelled: 'error',
  Returned: 'error',
  UnPacked: 'warning',
  UnSupplied: 'error',
};

function StatCard({
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
        flex: '1 1 180px',
        display: 'flex',
        gap: 1.75,
        alignItems: 'center',
        bgcolor: 'background.paper',
        border: '1px solid',
        borderColor: 'divider',
        borderRadius: 3,
        p: 2.25,
      }}
    >
      <Box
        sx={{
          width: 42,
          height: 42,
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
        <Typography variant="body2" color="text.secondary" noWrap>
          {label}
        </Typography>
        <Typography sx={{ fontWeight: 700, fontSize: '1.3rem' }} noWrap>
          {value}
        </Typography>
      </Box>
    </Box>
  );
}

export function CourierDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [stats, setStats] = useState<CourierStats | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getCourierStats(Number(id))
      .then(setStats)
      .catch((e) => setError(e instanceof Error ? e.message : 'Kurye analizi yüklenemedi.'));
  }, [id]);

  return (
    <Box>
      <Stack direction="row" spacing={1.5} alignItems="center" sx={{ mb: 3 }}>
        <IconButton onClick={() => navigate('/couriers')} aria-label="Geri">
          <ArrowBackIcon />
        </IconButton>
        <Box>
          <Typography variant="h1">{stats?.courierName ?? 'Kurye Analizi'}</Typography>
          <Typography variant="body2" color="text.secondary">
            Kurye Performans ve Ciro Analizi
          </Typography>
        </Box>
      </Stack>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {!stats && !error && (
        <Stack direction="row" spacing={1} alignItems="center" sx={{ py: 4 }}>
          <CircularProgress size={18} />
          <Typography variant="body2">Yükleniyor...</Typography>
        </Stack>
      )}

      {stats && (
        <>
          <Stack direction="row" flexWrap="wrap" gap={2} sx={{ mb: 4 }}>
            <StatCard icon={<Inventory2OutlinedIcon />} label="Toplam Paket" value={stats.totalOrders.toString()} bg="#e5f0fd" color="#0b6e99" />
            <StatCard icon={<CheckCircleOutlineIcon />} label="Teslim Edilen" value={stats.deliveredOrders.toString()} bg="#dcf3e4" color="#1f9254" />
            <StatCard icon={<CancelOutlinedIcon />} label="İptal" value={stats.cancelledOrders.toString()} bg="#fbe7e7" color="#d63b3b" />
            <StatCard icon={<AssignmentReturnOutlinedIcon />} label="İade" value={stats.returnedOrders.toString()} bg="#fdecc8" color="#b5730c" />
            <StatCard icon={<PaidOutlinedIcon />} label="Toplam Ciro" value={stats.totalRevenue.toFixed(2)} bg="#e5f0fd" color="#0b6e99" />
          </Stack>

          <Typography variant="h2" sx={{ mb: 2 }}>
            Son Siparişler
          </Typography>
          <Paper variant="outlined">
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Sipariş No</TableCell>
                    <TableCell>Tarih</TableCell>
                    <TableCell>Durum</TableCell>
                    <TableCell align="right">Tutar</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {stats.recentOrders.map((order) => (
                    <TableRow key={order.id} hover>
                      <TableCell>{order.orderNumber}</TableCell>
                      <TableCell>{new Date(order.orderDate).toLocaleString('tr-TR')}</TableCell>
                      <TableCell>
                        <Chip size="small" label={order.status} color={STATUS_COLOR[order.status] ?? 'default'} />
                      </TableCell>
                      <TableCell align="right">{order.invoiceAmount != null ? order.invoiceAmount.toFixed(2) : '-'}</TableCell>
                    </TableRow>
                  ))}
                  {stats.recentOrders.length === 0 && (
                    <TableRow>
                      <TableCell colSpan={4}>
                        <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                          Bu kuryeye henüz sipariş atanmamış.
                        </Typography>
                      </TableCell>
                    </TableRow>
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        </>
      )}
    </Box>
  );
}
