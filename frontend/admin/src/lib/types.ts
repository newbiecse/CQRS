export type Product = {
  id: string;
  name: string;
  price: number;
  createdAt: string;
  lastUpdatedAt: string;
};

export type User = {
  id: string;
  email: string;
  displayName: string;
  isActive: boolean;
  registeredAt: string;
  lastUpdatedAt: string;
};

export type OrderLine = {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
};

export type Order = {
  id: string;
  customerId: string;
  cartId: string;
  status: string;
  lines: OrderLine[];
  totalAmount: number;
  paymentId: string | null;
  createdAt: string;
  lastUpdatedAt: string;
};

export type Cart = {
  id: string;
  customerId: string;
  status: string;
  items: OrderLine[];
  subtotal: number;
  createdAt: string;
  lastUpdatedAt: string;
};

export type TopUserReport = {
  userId: string;
  email: string;
  displayName: string;
  totalOrderAmount: number;
  orderCount: number;
  period: string;
  periodStartUtc: string;
  periodEndUtc: string;
};

export type CheckoutSaga = {
  id: string;
  cartId: string;
  orderId: string | null;
  paymentId: string | null;
  simulatePaymentFailure: boolean;
  state: string;
  failureReason: string | null;
  createdAt: string;
  updatedAt: string;
};

export type ApiError = {
  message: string;
};
