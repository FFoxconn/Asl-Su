import { Fragment, useEffect, useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import { createBrand, createCategory, createStore, getBrands, getCategories, getStores } from '../../api/catalog';
import { createProduct, getProducts } from '../../api/products';
import { pushProducts } from '../../api/trendyolSync';
import type { Brand, Category, Store } from '../../types/catalog';
import type { Product } from '../../types/product';
import type { ProductSyncSummary } from '../../types/trendyolSync';
import { StockPriceEditor } from './StockPriceEditor';

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
      setError(e instanceof Error ? e.message : 'Trendyol Go\'ya aktarım başarısız oldu.');
    } finally {
      setIsPushing(false);
    }
  }

  return (
    <div style={{ maxWidth: 900, margin: '2rem auto', fontFamily: 'system-ui' }}>
      <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1>Ürünler</h1>
        <Link to="/">Genel Bakışa Dön</Link>
      </header>

      {error && <p style={{ color: 'crimson' }}>{error}</p>}

      <section style={{ display: 'flex', gap: 24, marginBottom: 24, fontSize: 14 }}>
        <form onSubmit={handleCreateStore}>
          <strong>Şube Ekle</strong>
          <div>
            <input value={newStoreName} onChange={(e) => setNewStoreName(e.target.value)} placeholder="Şube adı" />
            <button type="submit">Ekle</button>
          </div>
        </form>
        <form onSubmit={handleCreateCategory}>
          <strong>Kategori Ekle</strong>
          <div>
            <input value={newCategoryName} onChange={(e) => setNewCategoryName(e.target.value)} placeholder="Kategori adı" />
            <button type="submit">Ekle</button>
          </div>
        </form>
        <form onSubmit={handleCreateBrand}>
          <strong>Marka Ekle</strong>
          <div>
            <input value={newBrandName} onChange={(e) => setNewBrandName(e.target.value)} placeholder="Marka adı" />
            <button type="submit">Ekle</button>
          </div>
        </form>
      </section>

      <section style={{ marginBottom: 24 }}>
        <h2>Yeni Ürün</h2>
        <form onSubmit={handleCreateProduct} style={{ display: 'flex', gap: 8, flexWrap: 'wrap', alignItems: 'center' }}>
          <input value={sku} onChange={(e) => setSku(e.target.value)} placeholder="SKU" required />
          <input value={barcode} onChange={(e) => setBarcode(e.target.value)} placeholder="Barkod" required />
          <input value={name} onChange={(e) => setName(e.target.value)} placeholder="Ürün adı" required />
          <input
            type="number"
            value={vatRate}
            onChange={(e) => setVatRate(e.target.value)}
            placeholder="KDV %"
            style={{ width: 70 }}
          />
          <select value={categoryId} onChange={(e) => setCategoryId(e.target.value ? Number(e.target.value) : '')}>
            <option value="">Kategori seç...</option>
            {categories.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>
          <select value={brandId} onChange={(e) => setBrandId(e.target.value ? Number(e.target.value) : '')}>
            <option value="">Marka seç...</option>
            {brands.map((b) => (
              <option key={b.id} value={b.id}>
                {b.name}
              </option>
            ))}
          </select>
          <button type="submit">Ürün Ekle</button>
        </form>
      </section>

      <section>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <h2>Ürün Listesi ({products.length})</h2>
          <button onClick={handlePushProducts} disabled={isPushing}>
            {isPushing ? 'Aktarılıyor...' : 'Trendyol Go\'ya Aktar'}
          </button>
        </div>
        {syncSummary && (
          <p style={{ color: syncSummary.failedCount > 0 ? '#b45309' : 'green' }}>
            {syncSummary.message}
          </p>
        )}
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ borderBottom: '1px solid #ccc', textAlign: 'left' }}>
              <th>Ad</th>
              <th>SKU</th>
              <th>Barkod</th>
              <th>Aktif</th>
              <th>Trendyol Go Durumu</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <Fragment key={product.id}>
                <tr style={{ borderBottom: '1px solid #eee' }}>
                  <td>{product.name}</td>
                  <td>{product.sku}</td>
                  <td>{product.barcode}</td>
                  <td>{product.isActive ? 'Evet' : 'Hayır'}</td>
                  <td>{product.tgoSyncStatus}</td>
                  <td>
                    <button
                      onClick={() =>
                        setExpandedProductId(expandedProductId === product.id ? null : product.id)
                      }
                    >
                      {expandedProductId === product.id ? 'Kapat' : 'Stok/Fiyat'}
                    </button>
                  </td>
                </tr>
                {expandedProductId === product.id && (
                  <tr>
                    <td colSpan={6}>
                      <StockPriceEditor productId={product.id} stores={stores} />
                    </td>
                  </tr>
                )}
              </Fragment>
            ))}
            {products.length === 0 && (
              <tr>
                <td colSpan={6}>Henüz ürün eklenmemiş.</td>
              </tr>
            )}
          </tbody>
        </table>
      </section>
    </div>
  );
}
