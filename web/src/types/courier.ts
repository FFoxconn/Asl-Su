export interface Courier {
  id: number;
  name: string;
  phone: string | null;
  isActive: boolean;
  hasLogin: boolean;
}

export interface CourierOrderSummary {
  id: number;
  orderNumber: string;
  orderDate: string;
  status: string;
  invoiceAmount: number | null;
}

export interface CourierStats {
  courierId: number;
  courierName: string;
  totalOrders: number;
  deliveredOrders: number;
  cancelledOrders: number;
  returnedOrders: number;
  totalRevenue: number;
  recentOrders: CourierOrderSummary[];
}