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
