import { useEffect, useState } from 'react';
import { getStockPrice, setSaleStatus, upsertStockPrice } from '../../api/products';
import type { Store } from '../../types/catalog';
import type { StockPrice } from '../../types/product';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import MenuItem from '@mui/material/MenuItem';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';

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
        reasonCode: row.isOnSale ? reasonDrafts[row.storeId] || null : null,
      });
      refresh();
    } finally {
      setTogglingStoreId(null);
    }
  }

  return (
    <Box sx={{ bgcolor: 'action.hover', borderRadius: 1, p: 2 }}>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Şube</TableCell>
            <TableCell align="right">Stok</TableCell>
            <TableCell align="right">Satış Fiyatı</TableCell>
            <TableCell align="right">Liste Fiyatı</TableCell>
            <TableCell>Satış Durumu</TableCell>
            <TableCell align="right" />
          </TableRow>
        </TableHead>
        <TableBody>
          {rows.map((row) => (
            <TableRow key={row.storeId}>
              <TableCell>{row.storeName}</TableCell>
              <TableCell align="right">{row.quantity}</TableCell>
              <TableCell align="right">{row.salePrice.toFixed(2)}</TableCell>
              <TableCell align="right">{row.listPrice.toFixed(2)}</TableCell>
              <TableCell>
                {row.isOnSale ? (
                  <Chip size="small" color="success" label="Satışta" />
                ) : (
                  <Chip size="small" color="error" label={`Satışta Değil${row.unsaleReasonCode ? ` (${row.unsaleReasonCode})` : ''}`} />
                )}
              </TableCell>
              <TableCell align="right">
                <Stack direction="row" spacing={1} justifyContent="flex-end" alignItems="center">
                  {row.isOnSale && (
                    <TextField
                      size="small"
                      variant="standard"
                      placeholder="Neden kodu"
                      value={reasonDrafts[row.storeId] ?? ''}
                      onChange={(e) => setReasonDrafts((prev) => ({ ...prev, [row.storeId]: e.target.value }))}
                      sx={{ width: 110 }}
                    />
                  )}
                  <Button
                    size="small"
                    variant="outlined"
                    color={row.isOnSale ? 'error' : 'success'}
                    onClick={() => handleToggleSaleStatus(row)}
                    disabled={togglingStoreId === row.storeId}
                  >
                    {togglingStoreId === row.storeId ? '...' : row.isOnSale ? 'Satıştan Kaldır' : 'Satışa Aç'}
                  </Button>
                </Stack>
              </TableCell>
            </TableRow>
          ))}
          {rows.length === 0 && (
            <TableRow>
              <TableCell colSpan={6}>
                <Typography variant="body2" color="text.secondary">
                  Bu ürün için henüz stok/fiyat girilmemiş.
                </Typography>
              </TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <Stack direction="row" spacing={1} alignItems="center" sx={{ mt: 2 }} flexWrap="wrap" useFlexGap>
        <TextField
          size="small"
          select
          value={storeId}
          onChange={(e) => setStoreId(e.target.value ? Number(e.target.value) : '')}
          label="Şube"
          sx={{ minWidth: 160 }}
        >
          <MenuItem value="">Şube seç...</MenuItem>
          {stores.map((s) => (
            <MenuItem key={s.id} value={s.id}>
              {s.name}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          size="small"
          type="number"
          value={quantity}
          onChange={(e) => setQuantity(e.target.value)}
          label="Stok"
          sx={{ width: 90 }}
        />
        <TextField
          size="small"
          type="number"
          value={salePrice}
          onChange={(e) => setSalePrice(e.target.value)}
          label="Satış Fiyatı"
          sx={{ width: 120 }}
        />
        <TextField
          size="small"
          type="number"
          value={listPrice}
          onChange={(e) => setListPrice(e.target.value)}
          label="Liste Fiyatı"
          sx={{ width: 120 }}
        />
        <Button variant="contained" size="small" onClick={handleSave} disabled={isSaving || storeId === ''}>
          {isSaving ? 'Kaydediliyor...' : 'Kaydet'}
        </Button>
      </Stack>
      <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
        Neden kodu Trendyol Go'nun resmi olarak desteklediği bir değer olmalı (developers.tgoapps.com).
      </Typography>
    </Box>
  );
}
