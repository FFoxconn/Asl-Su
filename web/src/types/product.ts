export interface Product {
  id: number;
  sku: string;
  barcode: string;
  name: string;
  description: string | null;
  categoryId: number | null;
  brandId: number | null;
  vatRate: number;
  imageUrl: string | null;
  isActive: boolean;
  tgoSyncStatus: string;
}

export interface CreateProductRequest {
  sku: string;
  barcode: string;
  name: string;
  description: string | null;
  categoryId: number | null;
  brandId: number | null;
  vatRate: number;
  imageUrl: string | null;
}

export interface StockPrice {
  productId: number;
  storeId: number;
  storeName: string;
  quantity: number;
  salePrice: number;
  listPrice: number;
  lastSyncedAt: string | null;
}

export interface UpsertStockPriceRequest {
  productId: number;
  storeId: number;
  quantity: number;
  salePrice: number;
  listPrice: number;
}
