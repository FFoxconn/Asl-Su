import { Fragment, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { getOrder, getOrders } from '../../api/orders';
import { pullOrders } from '../../api/trendyolSync';
import type { OrderDetail, OrderListItem } from '../../types/order';
import type { OrderSyncSummary } from '../../types/trendyolSync';

const TABS = [
  { key: 'all', label: 'Tüm Siparişler' },
  { key: 'New', label: 'Yeni Siparişler' },
  { key: 'Preparing', label: 'Hazırlananlar' },
  { key: 'Delivered', label: 'Teslim Edilenler' },
  { key: 'Cancelled', label: 'İptaller' },
] as const;

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
    <div style={{ maxWidth: 960, margin: '2rem auto', fontFamily: 'system-ui' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Siparişler</h1>
        <Link to="/">Genel Bakışa Dön</Link>
      </header>

      {error && <p style={{ color: 'crimson' }}>{error}</p>}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 12 }}>
        <nav style={{ display: 'flex', gap: 4 }}>
          {TABS.map((t) => (
            <button
              key={t.key}
              onClick={() => setTab(t.key)}
              style={{
                padding: '6px 12px',
                border: '1px solid #ccc',
                background: tab === t.key ? '#333' : '#fff',
                color: tab === t.key ? '#fff' : '#333',
                cursor: 'pointer',
              }}
            >
              {t.label}
            </button>
          ))}
        </nav>
        <button onClick={handlePullOrders} disabled={isPulling}>
          {isPulling ? 'Çekiliyor...' : 'Siparişleri Çek'}
        </button>
      </div>

      {pullSummary && (
        <p style={{ color: pullSummary.skippedCount > 0 ? '#b45309' : 'green' }}>{pullSummary.message}</p>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ borderBottom: '1px solid #ccc', textAlign: 'left' }}>
            <th>Sipariş No</th>
            <th>Paket ID</th>
            <th>Müşteri</th>
            <th>Tarih</th>
            <th>Tutar</th>
            <th>Durum</th>
            <th>İş Akışı</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {filtered.map((order) => (
            <Fragment key={order.id}>
              <tr style={{ borderBottom: '1px solid #eee' }}>
                <td>{order.orderNumber}</td>
                <td>{order.packageId}</td>
                <td>{order.customerName ?? '-'}</td>
                <td>{new Date(order.orderDate).toLocaleString()}</td>
                <td>{order.invoiceAmount != null ? order.invoiceAmount.toFixed(2) : '-'}</td>
                <td>{order.status}</td>
                <td>{order.workflowStatus}</td>
                <td>
                  <button onClick={() => toggleExpand(order)}>
                    {expandedOrderId === order.id ? 'Kapat' : 'Detay'}
                  </button>
                </td>
              </tr>
              {expandedOrderId === order.id && (
                <tr>
                  <td colSpan={8}>
                    {expandedDetail ? (
                      <div style={{ padding: '8px 16px', background: '#fafafa', border: '1px solid #eee' }}>
                        <p>
                          {expandedDetail.customerPhone && <span>Tel: {expandedDetail.customerPhone} </span>}
                          {expandedDetail.customerAddress && <span>Adres: {expandedDetail.customerAddress}</span>}
                        </p>
                        <table style={{ width: '100%' }}>
                          <thead>
                            <tr>
                              <th style={{ textAlign: 'left' }}>Barkod</th>
                              <th style={{ textAlign: 'right' }}>Adet</th>
                              <th style={{ textAlign: 'right' }}>Birim Fiyat</th>
                              <th>Eşleşen Ürün</th>
                            </tr>
                          </thead>
                          <tbody>
                            {expandedDetail.items.map((item) => (
                              <tr key={item.id}>
                                <td>{item.barcode}</td>
                                <td style={{ textAlign: 'right' }}>{item.quantity}</td>
                                <td style={{ textAlign: 'right' }}>{item.unitPrice.toFixed(2)}</td>
                                <td>{item.productId ? `#${item.productId}` : 'Eşleşmedi'}</td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    ) : (
                      <p>Yükleniyor...</p>
                    )}
                  </td>
                </tr>
              )}
            </Fragment>
          ))}
          {filtered.length === 0 && (
            <tr>
              <td colSpan={8}>Bu kategoride sipariş bulunmuyor.</td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
