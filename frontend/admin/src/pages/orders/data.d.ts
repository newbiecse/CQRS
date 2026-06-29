export type OrderLine = {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
};

export type OrderItem = {
  id: string;
  customerId: string;
  cartId: string;
  status: 'PendingPayment' | 'Paid' | 'Cancelled' | string;
  lines: OrderLine[];
  totalAmount: number;
  paymentId?: string | null;
  createdAt: string;
  lastUpdatedAt: string;
};

export type OrderLineInput = {
  productId: string;
  productName: string;
  unitPrice: number;
  quantity: number;
};

export type CreateOrderPayload = {
  customerId: string;
  lines: OrderLineInput[];
};

export type UpdateOrderPayload = {
  lines: OrderLineInput[];
};

export type InventoryItem = {
  productId: string;
  productName: string;
  onHand: number;
  reserved: number;
  available: number;
  lastUpdatedAt: string;
};
