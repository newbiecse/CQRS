import { request } from '@umijs/max';
import type {
  CreateOrderPayload,
  InventoryItem,
  OrderItem,
  UpdateOrderPayload,
} from './data.d';

const adminOpts = { skipErrorHandler: true };

export async function queryOrders(): Promise<{
  data: OrderItem[];
  total: number;
  success: boolean;
}> {
  const data = await request<OrderItem[]>('/api/admin/orders', {
    method: 'GET',
    ...adminOpts,
  });
  return { data: data ?? [], total: data?.length ?? 0, success: true };
}

export async function getOrderById(id: string) {
  return request<OrderItem>(`/api/admin/orders/${id}`, {
    method: 'GET',
    ...adminOpts,
  });
}

export async function createOrder(body: CreateOrderPayload) {
  return request('/api/admin/orders', {
    method: 'POST',
    data: body,
    ...adminOpts,
  });
}

export async function updateOrder(id: string, body: UpdateOrderPayload) {
  return request(`/api/admin/orders/${id}`, {
    method: 'PUT',
    data: body,
    ...adminOpts,
  });
}

export async function deleteOrder(id: string) {
  return request(`/api/admin/orders/${id}`, {
    method: 'DELETE',
    ...adminOpts,
  });
}

export async function cancelOrder(id: string, reason: string) {
  return request(`/api/admin/orders/${id}/cancel`, {
    method: 'POST',
    data: { reason },
    ...adminOpts,
  });
}

export async function markOrderPaid(id: string, paymentId: string, amount: number) {
  return request(`/api/admin/orders/${id}/mark-paid`, {
    method: 'POST',
    data: { paymentId, amount },
    ...adminOpts,
  });
}

export async function queryInventory(): Promise<InventoryItem[]> {
  return request<InventoryItem[]>('/api/admin/inventory', {
    method: 'GET',
    ...adminOpts,
  });
}

export async function adjustInventory(productId: string, onHand: number) {
  return request(`/api/admin/inventory/${productId}`, {
    method: 'PUT',
    data: { onHand },
    ...adminOpts,
  });
}
