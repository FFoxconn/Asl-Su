import { Fragment, useEffect, useMemo, useState } from 'react';
import {
  acceptOrder,
  assignCourier,
  deliverOrder,
  getOrder,
  getOrders,
  markOrderPrepared,
  startPreparingOrder,
  substituteOrderItem,
} from '../../api/orders';
import { getCouriers } from '../../api/couriers';
import { getProducts } from '../../api/products';
import { pullOrders } from '../../api/trendyolSync';
import type { OrderDetail, OrderListItem, OrderWorkflowActionResult } from '../../types/order';
import type { Courier } from '../../types/courier';
import type { Product } from '../../types/product';
import type { OrderSyncSummary } from '../../types/trendyolSync';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import CircularProgress from '@mui/material/CircularProgress';
import Collapse from '@mui/material/Collapse';
import IconButton from '@mui/material/IconButton';
import MenuItem from '@mui/material/MenuItem';
import Paper from '@mui/material/Paper';
import Select from '@mui/material/Select';
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
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import CheckIcon from '@mui/icons-material/Check';

const WORKFLOW_ACTIONS: Record<string, { label: string; call: (id: number) => Promise<OrderWorkflowActionResult> }> = {
  New: { label: 'Kabul Et', call: acceptOrder },
  Accepted: { label: 'Hazırlanmaya Başla', call: startPreparingOrder },
  Preparing: { label: 'Hazırlandı Olarak İşaretle', call: markOrderPrepared },
  Prepared: { label: 'Teslim Et', call: deliverOrder },
};

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
  const [couriers, setCouriers] = useState<Courier[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [tab, setTab] = useState<(typeof TABS)[number]['key']>('all');
  const [isPulling, setIsPulling] = useState(false);
  const [pullSummary, setPullSummary] = useState<OrderSyncSummary | null>(null);
  const [expandedOrderId, setExpandedOrderId] = useState<number | null>(null);
  const [expandedDetail, setExpandedDetail] = useState<OrderDetail | null>(null);
  const [actioningOrderId, setActioningOrderId] = useState<number | null>(null);
  const [actionMessage, setActionMessage] = useState<{ orderId: number; text: string; ok: boolean } | null>(null);
  const [assigningCourierOrderId, setAssigningCourierOrderId] = useState<number | null>(null);
  const [substituteDrafts, setSubstituteDrafts] = useState<Record<number, number | ''>>({});
  const [substitutingItemId, setSubstitutingItemId] = useState<number | null>(null);

  function refresh() {
    getOrders().then(setOrders).catch((e) => setError(e instanceof Error ? e.message : 'Siparişler yüklenemedi.'));
  }

  useEffect(refresh, []);
  useEffect(() => {
    getCouriers().then(setCouriers).catch(() => undefined);
    getProducts().then(setProducts).catch(() => undefined);
  }, []);

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

  async function handleWorkflowAction(order: OrderListItem) {
    const action = WORKFLOW_ACTIONS[order.workflowStatus];
    if (!action) return;

    setActioningOrderId(order.id);
    setActionMessage(null);
    try {
      const result = await action.call(order.id);
      setActionMessage({
        orderId: order.id,
        ok: true,
        text: result.trendyolNotified
          ? 'Durum güncellendi ve Trendyol Go bilgilendirildi.'
          : `Durum güncellendi (Trendyol Go bilgilendirilemedi: ${result.trendyolMessage ?? 'bilinmeyen hata'}).`,
      });
      refresh();
      if (expandedOrderId === order.id) {
        setExpandedDetail(await getOrder(order.id));
      }
    } catch (e) {
      setActionMessage({ orderId: order.id, ok: false, text: e instanceof Error ? e.message : 'İşlem başarısız oldu.' });
    } finally {
      setActioningOrderId(null);
    }
  }

  async function handleAssignCourier(order: OrderListItem, courierId: number) {
    setAssigningCourierOrderId(order.id);
    setError(null);
    try {
      await assignCourier(order.id, courierId);
      refresh();
      if (expandedOrderId === order.id) {
        setExpandedDetail(await getOrder(order.id));
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Kurye atanamadı.');
    } finally {
      setAssigningCourierOrderId(null);
    }
  }

  async function handleSubstituteItem(orderId: number, itemId: number) {
    const productId = substituteDrafts[itemId];
    if (productId === '' || productId == null) return;

    setSubstitutingItemId(itemId);
    setError(null);
    try {
      const updated = await substituteOrderItem(orderId, itemId, productId);
      setExpandedDetail(updated);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'İkame ürün atanamadı.');
    } finally {
      setSubstitutingItemId(null);
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
                <TableCell>Kurye</TableCell>
                <TableCell>İşlem</TableCell>
                <TableCell align="right" />
              </TableRow>
            </TableHead>
            <TableBody>
              {filtered.map((order) => {
                const isExpanded = expandedOrderId === order.id;
                const workflowAction = WORKFLOW_ACTIONS[order.workflowStatus];
                const isActioning = actioningOrderId === order.id;
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
                      <TableCell>
                        <Select
                          size="small"
                          displayEmpty
                          value={order.courierId ?? ''}
                          onChange={(e) => handleAssignCourier(order, Number(e.target.value))}
                          disabled={assigningCourierOrderId === order.id}
                          sx={{ minWidth: 140 }}
                        >
                          <MenuItem value="" disabled>
                            Atanmadı
                          </MenuItem>
                          {couriers.map((c) => (
                            <MenuItem key={c.id} value={c.id}>
                              {c.name}
                            </MenuItem>
                          ))}
                        </Select>
                      </TableCell>
                      <TableCell>
                        {workflowAction ? (
                          <Button
                            size="small"
                            variant="outlined"
                            onClick={() => handleWorkflowAction(order)}
                            disabled={isActioning}
                          >
                            {isActioning ? '...' : workflowAction.label}
                          </Button>
                        ) : order.workflowStatus === 'Delivered' ? (
                          <Chip size="small" icon={<CheckCircleOutlineIcon />} label="Tamamlandı" color="success" variant="outlined" />
                        ) : null}
                      </TableCell>
                      <TableCell align="right">
                        <IconButton size="small" onClick={() => toggleExpand(order)} aria-label="Detay">
                          {isExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                        </IconButton>
                      </TableCell>
                    </TableRow>
                    {actionMessage && actionMessage.orderId === order.id && (
                      <TableRow>
                        <TableCell colSpan={10} sx={{ py: 0.5, border: 'none' }}>
                          <Alert severity={actionMessage.ok ? 'success' : 'error'} sx={{ py: 0 }}>
                            {actionMessage.text}
                          </Alert>
                        </TableCell>
                      </TableRow>
                    )}
                    <TableRow>
                      <TableCell colSpan={10} sx={{ py: 0, borderBottom: isExpanded ? undefined : 'none' }}>
                        <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                          <Box sx={{ py: 2, bgcolor: 'action.hover', borderRadius: 1, px: 2, my: 1 }}>
                            {expandedDetail ? (
                              <>
                                <Typography variant="body2" sx={{ mb: 1.5 }}>
                                  {expandedDetail.customerPhone && <span>Tel: {expandedDetail.customerPhone} &nbsp;&nbsp;</span>}
                                  {expandedDetail.customerAddress && <span>Adres: {expandedDetail.customerAddress} &nbsp;&nbsp;</span>}
                                  Kurye: {expandedDetail.courierName ?? 'Atanmadı'}
                                </Typography>
                                <Table size="small">
                                  <TableHead>
                                    <TableRow>
                                      <TableCell>Barkod</TableCell>
                                      <TableCell align="right">Adet</TableCell>
                                      <TableCell align="right">Birim Fiyat</TableCell>
                                      <TableCell>Eşleşen Ürün</TableCell>
                                      <TableCell>İkame Ürün</TableCell>
                                    </TableRow>
                                  </TableHead>
                                  <TableBody>
                                    {expandedDetail.items.map((item) => (
                                      <TableRow key={item.id}>
                                        <TableCell>{item.barcode}</TableCell>
                                        <TableCell align="right">{item.quantity}</TableCell>
                                        <TableCell align="right">{item.unitPrice.toFixed(2)}</TableCell>
                                        <TableCell>
                                          {item.isSubstitution ? (
                                            <Chip size="small" color="info" label={`#${item.productId} (ikame)`} />
                                          ) : item.productId ? (
                                            <Chip size="small" label={`#${item.productId}`} />
                                          ) : (
                                            <Chip size="small" variant="outlined" color="warning" label="Eşleşmedi" />
                                          )}
                                        </TableCell>
                                        <TableCell>
                                          <Stack direction="row" spacing={1} alignItems="center">
                                            <Select
                                              size="small"
                                              displayEmpty
                                              value={substituteDrafts[item.id] ?? ''}
                                              onChange={(e) =>
                                                setSubstituteDrafts((prev) => ({
                                                  ...prev,
                                                  [item.id]: e.target.value === '' ? '' : Number(e.target.value),
                                                }))
                                              }
                                              sx={{ minWidth: 160 }}
                                            >
                                              <MenuItem value="">Ürün seç...</MenuItem>
                                              {products.map((p) => (
                                                <MenuItem key={p.id} value={p.id}>
                                                  {p.name}
                                                </MenuItem>
                                              ))}
                                            </Select>
                                            <IconButton
                                              size="small"
                                              aria-label="İkame Ürünü Uygula"
                                              disabled={substitutingItemId === item.id || substituteDrafts[item.id] == null || substituteDrafts[item.id] === ''}
                                              onClick={() => handleSubstituteItem(expandedDetail.id, item.id)}
                                            >
                                              <CheckIcon fontSize="small" />
                                            </IconButton>
                                          </Stack>
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
                  <TableCell colSpan={10}>
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
