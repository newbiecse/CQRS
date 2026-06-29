export type ReportPeriod = 'day' | 'week' | 'month' | 'year';

export type TopBuyerItem = {
  userId: string;
  email: string;
  displayName: string;
  totalOrderAmount: number;
  orderCount: number;
  period: number;
  periodStartUtc: string;
  periodEndUtc: string;
};

export type TopProductItem = {
  productId: string;
  productName: string;
  totalQuantity: number;
  totalSalesAmount: number;
  orderCount: number;
  period: number;
  periodStartUtc: string;
  periodEndUtc: string;
};
