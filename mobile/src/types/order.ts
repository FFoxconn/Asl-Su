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
  courierId: number | null;
  courierName: string | null;
}
