export interface OrderListItem {
  id: number;
  packageId: string;
  orderNumber: string;
  storeId: number;
  status: string;
  workflowStatus: string;
  orderDate: string;
  invoiceAmount: number | null;
  customerName: string | null;
}

export interface OrderItem {
  id: number;
  productId: number | null;
  barcode: string;
  quantity: number;
  unitPrice: number;
  isSubstitution: boolean;
  substitutedForBarcode: string | null;
}

export interface OrderWorkflowActionResult {
  success: boolean;
  workflowStatus: string | null;
  trendyolNotified: boolean;
  trendyolMessage: string | null;
}

export interface OrderDetail {
  id: number;
  packageId: string;
  orderNumber: string;
  storeId: number;
  status: string;
  workflowStatus: string;
  orderDate: string;
  invoiceAmount: number | null;
  invoiceTaxAmount: number | null;
  bagCount: number | null;
  receiptLink: string | null;
  customerName: string | null;
  customerPhone: string | null;
  customerAddress: string | null;
  items: OrderItem[];
}
