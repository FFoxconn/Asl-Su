import { useEffect, useState } from 'react';
import { getStockPrice, setSaleStatus, upsertStockPrice } from '../../api/products';
import type { Store } from '../../types/catalog';
import type { StockPrice } from '../../types/product';

export function StockPriceEditor({ productId, stores }: { productId: number; stores: Store[] }) {
  const [rows, setRows] = useState<StockPrice[]>([]);
  const [storeId, setStoreId] = useState<number | ''>('');
  const [quantity, setQuantity] = useState('0');
  const [salePrice, setSalePrice] = useState('0');
  const [listPrice, setListPrice] = useState('0');
  const [isSaving, setIsSaving] = useState(false);
  const [reasonDrafts, setReasonDrafts] = useState<Record<number, string>>({});
  const [togglingStoreId, setTogglingStoreId] = useState<number | null>(null);

  function refresh() {
    getStockPrice(productId).then(setRows);
  }

  useEffect(refresh, [productId]);

  async function handleSave() {
    if (storeId === '') return;
    setIsSaving(true);
    try {
      await upsertStockPrice({
        productId,
        storeId,
        quantity: Number(quantity),
        salePrice: Number(salePrice),
        listPrice: Number(listPrice),
      });
      refresh();
    } finally {
      setIsSaving(false);
    }
  }

  async function handleToggleSaleStatus(row: StockPrice) {
    setTogglingStoreId(row.storeId);
    try {
      await setSaleStatus({
        productId,
        storeId: row.storeId,
        isOnSale: !row.isOnSale,
        reasonCode: row.isOnSale ? (reasonDrafts[row.storeId] || null) : null,
      });
      refresh();
    } finally {
      setTogglingStoreId(null);
    }
  }

  return (
    <div style={{ padding: '8px 16px', background: '#fafafa', border: '1px solid #eee' }}>
      <table style={{ width: '100%', marginBottom: 8 }}>
        <thead>
          <tr>
            <th style={{ textAlign: 'left' }}>Şube</th>
            <th style={{ textAlign: 'right' }}>Stok</th>
            <th style={{ textAlign: 'right' }}>Satış Fiyatı</th>
            <th style={{ textAlign: 'right' }}>Liste Fiyatı</th>
            <th style={{ textAlign: 'left' }}>Satış Durumu</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.storeId}>
              <td>{row.storeName}</td>
              <td style={{ textAlign: 'right' }}>{row.quantity}</td>
              <td style={{ textAlign: 'right' }}>{row.salePrice.toFixed(2)}</td>
              <td style={{ textAlign: 'right' }}>{row.listPrice.toFixed(2)}</td>
              <td>
                {row.isOnSale ? (
                  <span style={{ color: 'green' }}>Satışta</span>
                ) : (
                  <span style={{ color: 'crimson' }}>
                    Satışta Değil{row.unsaleReasonCode ? ` (${row.unsaleReasonCode})` : ''}
                  </span>
                )}
              </td>
              <td style={{ display: 'flex', gap: 4, alignItems: 'center' }}>
                {row.isOnSale && (
                  <input
                    type="text"
                    placeholder="Neden kodu"
                    value={reasonDrafts[row.storeId] ?? ''}
                    onChange={(e) => setReasonDrafts((prev) => ({ ...prev, [row.storeId]: e.target.value }))}
                    style={{ width: 90 }}
                  />
                )}
                <button onClick={() => handleToggleSaleStatus(row)} disabled={togglingStoreId === row.storeId}>
                  {togglingStoreId === row.storeId ? '...' : row.isOnSale ? 'Satıştan Kaldır' : 'Satışa Aç'}
                </button>
              </td>
            </tr>
          ))}
          {rows.length === 0 && (
            <tr>
              <td colSpan={6}>Bu ürün için henüz stok/fiyat girilmemiş.</td>
            </tr>
          )}
        </tbody>
      </table>

      <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
        <select value={storeId} onChange={(e) => setStoreId(e.target.value ? Number(e.target.value) : '')}>
          <option value="">Şube seç...</option>
          {stores.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
            </option>
          ))}
        </select>
        <input
          type="number"
          value={quantity}
          onChange={(e) => setQuantity(e.target.value)}
          placeholder="Stok"
          style={{ width: 80 }}
        />
        <input
          type="number"
          value={salePrice}
          onChange={(e) => setSalePrice(e.target.value)}
          placeholder="Satış Fiyatı"
          style={{ width: 100 }}
        />
        <input
          type="number"
          value={listPrice}
          onChange={(e) => setListPrice(e.target.value)}
          placeholder="Liste Fiyatı"
          style={{ width: 100 }}
        />
        <button onClick={handleSave} disabled={isSaving || storeId === ''}>
          {isSaving ? 'Kaydediliyor...' : 'Kaydet'}
        </button>
      </div>
      <p style={{ fontSize: 12, color: '#666', marginTop: 8 }}>
        Neden kodu Trendyol Go'nun resmi olarak desteklediği bir değer olmalı (developers.tgoapps.com).
      </p>
    </div>
  );
}
