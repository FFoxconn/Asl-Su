import { useEffect, useState } from 'react';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import LinearProgress from '@mui/material/LinearProgress';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Typography from '@mui/material/Typography';
import { getCouriers, getCourierStats } from '../../api/couriers';
import type { CourierStats } from '../../types/courier';

const MEDALS = ['🥇', '🥈', '🥉'];

export function CourierLeaderboardPage() {
  const [stats, setStats] = useState<CourierStats[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getCouriers()
      .then((couriers) => Promise.all(couriers.map((c) => getCourierStats(c.id))))
      .then((results) => {
        if (cancelled) return;
        const sorted = [...results].sort(
          (a, b) => b.deliveredOrders - a.deliveredOrders || b.totalRevenue - a.totalRevenue,
        );
        setStats(sorted);
      })
      .catch((e) => {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Kurye performansı yüklenemedi.');
      });
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <Box>
      <Typography variant="h1" gutterBottom>
        Kurye Performans Sıralaması
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Tüm zamanlar için teslim edilen sipariş sayısına göre sıralanmıştır.
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {stats == null && !error && (
        <Stack direction="row" spacing={1.5} alignItems="center" sx={{ py: 4 }}>
          <CircularProgress size={20} />
          <Typography variant="body2" color="text.secondary">
            Yükleniyor...
          </Typography>
        </Stack>
      )}

      {stats != null && (
        <Box sx={{ bgcolor: 'background.paper', border: '1px solid', borderColor: 'divider', borderRadius: 3, overflow: 'hidden' }}>
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell width={56} />
                  <TableCell>Kurye</TableCell>
                  <TableCell align="right">Teslim Edilen</TableCell>
                  <TableCell align="right">İptal</TableCell>
                  <TableCell align="right">İade</TableCell>
                  <TableCell align="right">Toplam Ciro</TableCell>
                  <TableCell sx={{ minWidth: 160 }}>Başarı Oranı</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {stats.map((s, index) => {
                  const successRate = s.totalOrders > 0 ? (s.deliveredOrders / s.totalOrders) * 100 : 0;
                  return (
                    <TableRow key={s.courierId} hover>
                      <TableCell sx={{ fontSize: '1.3rem', textAlign: 'center' }}>
                        {MEDALS[index] ?? <Typography color="text.secondary">{index + 1}</Typography>}
                      </TableCell>
                      <TableCell sx={{ fontWeight: 600 }}>{s.courierName}</TableCell>
                      <TableCell align="right">{s.deliveredOrders}</TableCell>
                      <TableCell align="right">{s.cancelledOrders}</TableCell>
                      <TableCell align="right">{s.returnedOrders}</TableCell>
                      <TableCell align="right">{s.totalRevenue.toFixed(2)} ₺</TableCell>
                      <TableCell>
                        <Stack direction="row" spacing={1} alignItems="center">
                          <LinearProgress
                            variant="determinate"
                            value={successRate}
                            sx={{
                              flex: 1,
                              height: 6,
                              borderRadius: 3,
                              bgcolor: 'action.hover',
                              '& .MuiLinearProgress-bar': { bgcolor: '#1f9254', borderRadius: 3 },
                            }}
                          />
                          <Typography variant="caption" sx={{ minWidth: 36, textAlign: 'right' }}>
                            %{successRate.toFixed(0)}
                          </Typography>
                        </Stack>
                      </TableCell>
                    </TableRow>
                  );
                })}
                {stats.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={7}>
                      <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                        Henüz kurye eklenmemiş.
                      </Typography>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        </Box>
      )}
    </Box>
  );
}
