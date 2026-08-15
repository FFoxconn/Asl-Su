import { Fragment, useEffect, useMemo, useState } from 'react';
import { getOrder, getOrders } from '../../api/orders';
import { pullOrders } from '../../api/trendyolSync';
import type { OrderDetail, OrderListItem } from '../../types/order';
import type { OrderSyncSummary } from '../../types/trendyolSync';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Collapse from '@mui/material/Collapse';
import IconButton from '@mui/material/IconButton';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Tab from '@mui/material/Tab';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Tabs from '@mui/material/Tabs';
import Typography from '@mui/material/Typography';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import DownloadOutlinedIcon from '@mui/icons-material/DownloadOutlined';

const TABS = [
  { key: 'all', label: 'Tüm Siparişler' },
  { key: 'New', label: 'Yeni Siparişler' },
  { key: 'Preparing', label: 'Hazırlananlar' },
  { key: 'Delivered', label: 'Teslim Edilenler' },
  { key: 'Cancelled', label: 'İptaller' },
] as const;

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

const WORKFLOW_COLOR: Record<string, 'default' | 'info' | 'warning' | 'primary' | 'success'> = {
  New: 'default',
  Accepted: 'info',
  Preparing: 'warning',
  Prepared: 'primary',
  Delivered: 'success',
};

export function OrdersPage() {
  const [orders, setOrders] = useState<OrderListItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<(typeof TABS)[number]['key']>('all');
  const [isPulling, setIsPulling] = useState(false);
  const [pullSummary, setPullSummary] = useState<OrderSyncSummary | null>(null);
  const [expandedOrderId, setExpandedOrderId] = useState<number | null>(null);
  const [expandedDetail, setExpandedDetail] = useState<OrderDetail | null>(null);

  function refresh() {
    getOrders().then(setOrders).catch((e) => setError(e instanceof Error ? e.message : 'Siparişler yüklenemedi.'));
  }

  useEffect(refresh, []);

  const filtered = useMemo(() => {
    if (tab === 'all') return orders;
    if (tab === 'Cancelled') return orders.filter((o) => o.status === 'Cancelled');
    return orders.filter((o) => o.workflowStatus === tab);
  }, [orders, tab]);

  async function handlePullOrders() {
    setIsPulling(true);
    setPullSummary(null);
    try {
      const summary = await pullOrders();
      setPullSummary(summary);
      refresh();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Siparişler çekilemedi.');
    } finally {
      setIsPulling(false);
    }
  }

  async function toggleExpand(order: OrderListItem) {
    if (expandedOrderId === order.id) {
      setExpandedOrderId(null);
      setExpandedDetail(null);
      return;
    }
    setExpandedOrderId(order.id);
    setExpandedDetail(null);
    try {
      setExpandedDetail(await getOrder(order.id));
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Sipariş detayı yüklenemedi.');
    }
  }

  return (
    <Box>
      <Typography variant="h1" gutterBottom>
        Siparişler
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Paper variant="outlined">
        <Stack
          direction={{ xs: 'column', sm: 'row' }}
          justifyContent="space-between"
          alignItems={{ sm: 'center' }}
          spacing={1.5}
          sx={{ px: 2, pt: 1.5 }}
        >
          <Tabs
            value={tab}
            onChange={(_, value) => setTab(value)}
            variant="scrollable"
            scrollButtons="auto"
            sx={{ minHeight: 40 }}
          >
            {TABS.map((t) => (
              <Tab key={t.key} value={t.key} label={t.label} sx={{ minHeight: 40 }} />
            ))}
          </Tabs>
          <Button
            variant="contained"
            size="small"
            startIcon={<DownloadOutlinedIcon />}
            onClick={handlePullOrders}
            disabled={isPulling}
            sx={{ flexShrink: 0 }}
          >
            {isPulling ? 'Çekiliyor...' : 'Siparişleri Çek'}
          </Button>
        </Stack>

        {pullSummary && (
          <Box sx={{ px: 2, pt: 1.5 }}>
            <Alert severity={pullSummary.skippedCount > 0 ? 'warning' : 'success'}>{pullSummary.message}</Alert>
          </Box>
        )}

        <TableContainer sx={{ mt: 1.5 }}>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Sipariş No</TableCell>
                <TableCell>Paket ID</TableCell>
                <TableCell>Müşteri</TableCell>
                <TableCell>Tarih</TableCell>
                <TableCell align="right">Tutar</TableCell>
                <TableCell>Durum</TableCell>
                <TableCell>İş Akışı</TableCell>
                <TableCell align="right" />
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((order) => {
                const isExpanded = expandedOrderId === order.id;
                return (
                  <Fragment key={order.id}>
                    <TableRow hover>
                      <TableCell>{order.orderNumber}</TableCell>
                      <TableCell>{order.packageId}</TableCell>
                      <TableCell>{order.customerName ?? '-'}</TableCell>
                      <TableCell>{new Date(order.orderDate).toLocaleString()}</TableCell>
                      <TableCell align="right">{order.invoiceAmount != null ? order.invoiceAmount.toFixed(2) : '-'}</TableCell>
                      <TableCell>
                        <Chip size="small" label={order.status} color={STATUS_COLOR[order.status] ?? 'default'} />
                      </TableCell>
                      <TableCell>
                        <Chip
                          size="small"
                          variant="outlined"
                          label={order.workflowStatus}
                          color={WORKFLOW_COLOR[order.workflowStatus] ?? 'default'}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => toggleExpand(order)} aria-label="Detay">
                          {isExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                        </IconButton>
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell colSpan={8} sx={{ py: 0, borderBottom: isExpanded ? undefined : 'none' }}>
                        <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                          <Box sx={{ py: 2, bgcolor: 'action.hover', borderRadius: 1, px: 2, my: 1 }}>
                            {expandedDetail ? (
                              <>
                                <Typography variant="body2" sx={{ mb: 1.5 }}>
                                  {expandedDetail.customerPhone && <span>Tel: {expandedDetail.customerPhone} &nbsp;&nbsp;</span>}
                                  {expandedDetail.customerAddress && <span>Adres: {expandedDetail.customerAddress}</span>}
                                </Typography>
                                <Table size="small">
                                  <TableHead>
                                    <TableRow>
                                      <TableCell>Barkod</TableCell>
                                      <TableCell align="right">Adet</TableCell>
                                      <TableCell align="right">Birim Fiyat</TableCell>
                                      <TableCell>Eşleşen Ürün</TableCell>
                                    </TableRow>
                                  </TableHead>
                                  <TableBody>
                                    {expandedDetail.items.map((item) => (
                                      <TableRow key={item.id}>
                                        <TableCell>{item.barcode}</TableCell>
                                        <TableCell align="right">{item.quantity}</TableCell>
                                        <TableCell align="right">{item.unitPrice.toFixed(2)}</TableCell>
                                        <TableCell>
                                          {item.productId ? (
                                            <Chip size="small" label={`#${item.productId}`} />
                                          ) : (
                                            <Chip size="small" variant="outlined" color="warning" label="Eşleşmedi" />
                                          )}
                                        </TableCell>
                                      </TableRow>
                                    ))}
                                  </TableBody>
                                </Table>
                              </>
                            ) : (
                              <Stack direction="row" spacing={1} alignItems="center">
                                <CircularProgress size={16} />
                                <Typography variant="body2">Yükleniyor...</Typography>
                              </Stack>
                            )}
                          </Box>
                        </Collapse>
                      </TableCell>
                    </TableRow>
                  </Fragment>
                );
              })}
              {filtered.length === 0 && (
                <TableRow>
                  <TableCell colSpan={8}>
                    <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                      Bu kategoride sipariş bulunmuyor.
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      </Paper>
    </Box>
  );
}
