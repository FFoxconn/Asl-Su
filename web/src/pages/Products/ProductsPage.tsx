import { Fragment, useEffect, useState, type FormEvent } from 'react';
import { createBrand, createCategory, createStore, getBrands, getCategories, getStores } from '../../api/catalog';
import { createProduct, getProducts } from '../../api/products';
import { pollBatchRequests, pushProducts, pushSaleStatus, pushStockPrice } from '../../api/trendyolSync';
import type { Brand, Category, Store } from '../../types/catalog';
import type { Product } from '../../types/product';
import type {
  BatchPollSummary,
  ProductSyncSummary,
  SaleStatusSyncSummary,
  StockPriceSyncSummary,
} from '../../types/trendyolSync';
import { StockPriceEditor } from './StockPriceEditor';
import Alert from '@mui/material/Alert';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Chip from '@mui/material/Chip';
import Collapse from '@mui/material/Collapse';
import Grid from '@mui/material/Grid';
import IconButton from '@mui/material/IconButton';
import MenuItem from '@mui/material/MenuItem';
import Paper from '@mui/material/Paper';
import Stack from '@mui/material/Stack';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import CloudUploadOutlinedIcon from '@mui/icons-material/CloudUploadOutlined';
import Inventory2OutlinedIcon from '@mui/icons-material/Inventory2Outlined';
import SellOutlinedIcon from '@mui/icons-material/SellOutlined';
import FactCheckOutlinedIcon from '@mui/icons-material/FactCheckOutlined';

const SYNC_STATUS_COLOR: Record<string, 'default' | 'warning' | 'success' | 'error'> = {
  NotSynced: 'default',
  Pending: 'warning',
  Synced: 'success',
  Failed: 'error',
};

export function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [stores, setStores] = useState<Store[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [expandedProductId, setExpandedProductId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const [sku, setSku] = useState('');
  const [barcode, setBarcode] = useState('');
  const [name, setName] = useState('');
  const [vatRate, setVatRate] = useState('18');
  const [categoryId, setCategoryId] = useState<number | ''>('');
  const [brandId, setBrandId] = useState<number | ''>('');
  const [newStoreName, setNewStoreName] = useState('');
  const [newCategoryName, setNewCategoryName] = useState('');
  const [newBrandName, setNewBrandName] = useState('');
  const [syncSummary, setSyncSummary] = useState<ProductSyncSummary | null>(null);
  const [isPushing, setIsPushing] = useState(false);
  const [stockPriceSummary, setStockPriceSummary] = useState<StockPriceSyncSummary | null>(null);
  const [isPushingStockPrice, setIsPushingStockPrice] = useState(false);
  const [pollSummary, setPollSummary] = useState<BatchPollSummary | null>(null);
  const [isPolling, setIsPolling] = useState(false);
  const [saleStatusSummary, setSaleStatusSummary] = useState<SaleStatusSyncSummary | null>(null);
  const [isPushingSaleStatus, setIsPushingSaleStatus] = useState(false);

  function refreshAll() {
    getProducts().then(setProducts).catch((e) => setError(String(e.message ?? e)));
    getStores().then(setStores);
    getCategories().then(setCategories);
    getBrands().then(setBrands);
  }

  useEffect(refreshAll, []);

  async function handleCreateProduct(event: FormEvent) {
    event.preventDefault();
    setError(null);
    try {
      await createProduct({
        sku,
        barcode,
        name,
        description: null,
        categoryId: categoryId === '' ? null : categoryId,
        brandId: brandId === '' ? null : brandId,
        vatRate: Number(vatRate),
        imageUrl: null,
      });
      setSku('');
      setBarcode('');
      setName('');
      refreshAll();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Ürün eklenemedi.');
    }
  }

  async function handleCreateStore(event: FormEvent) {
    event.preventDefault();
    if (!newStoreName.trim()) return;
    await createStore(newStoreName, newStoreName.toUpperCase().replace(/\s+/g, '-').slice(0, 50));
    setNewStoreName('');
    refreshAll();
  }

  async function handleCreateCategory(event: FormEvent) {
    event.preventDefault();
    if (!newCategoryName.trim()) return;
    await createCategory(newCategoryName);
    setNewCategoryName('');
    refreshAll();
  }

  async function handleCreateBrand(event: FormEvent) {
    event.preventDefault();
    if (!newBrandName.trim()) return;
    await createBrand(newBrandName);
    setNewBrandName('');
    refreshAll();
  }

  async function handlePushProducts() {
    setIsPushing(true);
    setSyncSummary(null);
    try {
      const summary = await pushProducts();
      setSyncSummary(summary);
      refreshAll();
    } catch (e) {
      setError(e instanceof Error ? e.message : "Trendyol Go'ya aktarım başarısız oldu.");
    } finally {
      setIsPushing(false);
    }
  }

  async function handlePushStockPrice() {
    setIsPushingStockPrice(true);
    setStockPriceSummary(null);
    try {
      setStockPriceSummary(await pushStockPrice());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Stok/fiyat aktarımı başarısız oldu.');
    } finally {
      setIsPushingStockPrice(false);
    }
  }

  async function handlePushSaleStatus() {
    setIsPushingSaleStatus(true);
    setSaleStatusSummary(null);
    try {
      setSaleStatusSummary(await pushSaleStatus());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Satış durumu aktarımı başarısız oldu.');
    } finally {
      setIsPushingSaleStatus(false);
    }
  }

  async function handlePollBatchRequests() {
    setIsPolling(true);
    setPollSummary(null);
    try {
      setPollSummary(await pollBatchRequests());
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Parti sonuçları kontrol edilemedi.');
    } finally {
      setIsPolling(false);
    }
  }

  return (
    <Box>
      <Typography variant="h1" gutterBottom>
        Ürünler
      </Typography>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Grid container spacing={2} sx={{ mb: 2 }}>
        <Grid item xs={12} sm={4}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="subtitle2" gutterBottom>
              Şube Ekle
            </Typography>
            <Box component="form" onSubmit={handleCreateStore} sx={{ display: 'flex', gap: 1 }}>
              <TextField
                size="small"
                value={newStoreName}
                onChange={(e) => setNewStoreName(e.target.value)}
                placeholder="Şube adı"
                fullWidth
              />
              <Button type="submit" variant="outlined">
                Ekle
              </Button>
            </Box>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="subtitle2" gutterBottom>
              Kategori Ekle
            </Typography>
            <Box component="form" onSubmit={handleCreateCategory} sx={{ display: 'flex', gap: 1 }}>
              <TextField
                size="small"
                value={newCategoryName}
                onChange={(e) => setNewCategoryName(e.target.value)}
                placeholder="Kategori adı"
                fullWidth
              />
              <Button type="submit" variant="outlined">
                Ekle
              </Button>
            </Box>
          </Paper>
        </Grid>
        <Grid item xs={12} sm={4}>
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Typography variant="subtitle2" gutterBottom>
              Marka Ekle
            </Typography>
            <Box component="form" onSubmit={handleCreateBrand} sx={{ display: 'flex', gap: 1 }}>
              <TextField
                size="small"
                value={newBrandName}
                onChange={(e) => setNewBrandName(e.target.value)}
                placeholder="Marka adı"
                fullWidth
              />
              <Button type="submit" variant="outlined">
                Ekle
              </Button>
            </Box>
          </Paper>
        </Grid>
      </Grid>

      <Paper variant="outlined" sx={{ p: 2, mb: 3 }}>
        <Typography variant="h2" gutterBottom>
          Yeni Ürün
        </Typography>
        <Box component="form" onSubmit={handleCreateProduct}>
          <Grid container spacing={1.5} alignItems="center">
            <Grid item xs={12} sm={2}>
              <TextField size="small" value={sku} onChange={(e) => setSku(e.target.value)} label="SKU" required fullWidth />
            </Grid>
            <Grid item xs={12} sm={2}>
              <TextField size="small" value={barcode} onChange={(e) => setBarcode(e.target.value)} label="Barkod" required fullWidth />
            </Grid>
            <Grid item xs={12} sm={3}>
              <TextField size="small" value={name} onChange={(e) => setName(e.target.value)} label="Ürün adı" required fullWidth />
            </Grid>
            <Grid item xs={6} sm={1.5}>
              <TextField
                size="small"
                type="number"
                value={vatRate}
                onChange={(e) => setVatRate(e.target.value)}
                label="KDV %"
                fullWidth
              />
            </Grid>
            <Grid item xs={6} sm={1.5}>
              <TextField
                size="small"
                select
                value={categoryId}
                onChange={(e) => setCategoryId(e.target.value ? Number(e.target.value) : '')}
                label="Kategori"
                fullWidth
              >
                <MenuItem value="">—</MenuItem>
                {categories.map((c) => (
                  <MenuItem key={c.id} value={c.id}>
                    {c.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={6} sm={1.5}>
              <TextField
                size="small"
                select
                value={brandId}
                onChange={(e) => setBrandId(e.target.value ? Number(e.target.value) : '')}
                label="Marka"
                fullWidth
              >
                <MenuItem value="">—</MenuItem>
                {brands.map((b) => (
                  <MenuItem key={b.id} value={b.id}>
                    {b.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid item xs={6} sm={0.5}>
              <Button type="submit" variant="contained" fullWidth>
                Ekle
              </Button>
            </Grid>
          </Grid>
        </Box>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} justifyContent="space-between" alignItems={{ md: 'center' }} spacing={1.5} sx={{ mb: 1.5 }}>
          <Typography variant="h2">Ürün Listesi ({products.length})</Typography>
          <Stack direction="row" spacing={1} flexWrap="wrap" useFlexGap>
            <Button size="small" variant="outlined" startIcon={<CloudUploadOutlinedIcon />} onClick={handlePushProducts} disabled={isPushing}>
              {isPushing ? 'Aktarılıyor...' : "Trendyol Go'ya Aktar"}
            </Button>
            <Button size="small" variant="outlined" startIcon={<Inventory2OutlinedIcon />} onClick={handlePushStockPrice} disabled={isPushingStockPrice}>
              {isPushingStockPrice ? 'Gönderiliyor...' : 'Stok/Fiyat Gönder'}
            </Button>
            <Button size="small" variant="outlined" startIcon={<SellOutlinedIcon />} onClick={handlePushSaleStatus} disabled={isPushingSaleStatus}>
              {isPushingSaleStatus ? 'Gönderiliyor...' : 'Satış Durumu Gönder'}
            </Button>
            <Button size="small" variant="outlined" startIcon={<FactCheckOutlinedIcon />} onClick={handlePollBatchRequests} disabled={isPolling}>
              {isPolling ? 'Kontrol ediliyor...' : 'Parti Sonuçlarını Kontrol Et'}
            </Button>
          </Stack>
        </Stack>

        <Stack spacing={1} sx={{ mb: 2 }}>
          {syncSummary && (
            <Alert severity={syncSummary.failedCount > 0 ? 'warning' : 'success'}>Ürünler: {syncSummary.message}</Alert>
          )}
          {stockPriceSummary && (
            <Alert severity={stockPriceSummary.failedCount > 0 ? 'warning' : 'success'}>
              Stok/Fiyat: {stockPriceSummary.message}
            </Alert>
          )}
          {saleStatusSummary && (
            <Alert severity={saleStatusSummary.failedCount > 0 ? 'warning' : 'success'}>
              Satış Durumu: {saleStatusSummary.message}
            </Alert>
          )}
          {pollSummary && (
            <Alert severity={pollSummary.failedCount > 0 ? 'warning' : 'success'}>
              Parti Kontrolü: {pollSummary.message} (Tamamlanan: {pollSummary.completedCount}, İşlemde:{' '}
              {pollSummary.stillProcessingCount}, Başarısız: {pollSummary.failedCount})
            </Alert>
          )}
        </Stack>

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>Ad</TableCell>
                <TableCell>SKU</TableCell>
                <TableCell>Barkod</TableCell>
                <TableCell>Aktif</TableCell>
                <TableCell>Trendyol Go Durumu</TableCell>
                <TableCell align="right" />
              </TableRow>
            </TableHead>
            <TableBody>
              {products.map((product) => {
                const isExpanded = expandedProductId === product.id;
                return (
                  <Fragment key={product.id}>
                    <TableRow hover>
                      <TableCell>{product.name}</TableCell>
                      <TableCell>{product.sku}</TableCell>
                      <TableCell>{product.barcode}</TableCell>
                      <TableCell>{product.isActive ? 'Evet' : 'Hayır'}</TableCell>
                      <TableCell>
                        <Chip
                          size="small"
                          label={product.tgoSyncStatus}
                          color={SYNC_STATUS_COLOR[product.tgoSyncStatus] ?? 'default'}
                        />
                      </TableCell>
                      <TableCell align="right">
                        <IconButton
                          size="small"
                          onClick={() => setExpandedProductId(isExpanded ? null : product.id)}
                          aria-label="Stok/Fiyat"
                        >
                          {isExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                        </IconButton>
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell colSpan={6} sx={{ py: 0, borderBottom: isExpanded ? undefined : 'none' }}>
                        <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                          <Box sx={{ py: 2 }}>
                            <StockPriceEditor productId={product.id} stores={stores} />
                          </Box>
                        </Collapse>
                      </TableCell>
                    </TableRow>
                  </Fragment>
                );
              })}
              {products.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6}>
                    <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                      Henüz ürün eklenmemiş.
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
