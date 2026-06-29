import { request } from '@umijs/max';
import type {
  CreateProductPayload,
  ProductItem,
  UpdateProductPayload,
} from './data.d';

const adminOpts = { skipErrorHandler: true };

export async function queryProducts(): Promise<{
  data: ProductItem[];
  total: number;
  success: boolean;
}> {
  const data = await request<ProductItem[]>('/api/admin/products', {
    method: 'GET',
    ...adminOpts,
  });
  return { data: data ?? [], total: data?.length ?? 0, success: true };
}

export async function getProductById(id: string) {
  return request<ProductItem>(`/api/admin/products/${id}`, {
    method: 'GET',
    ...adminOpts,
  });
}

export async function queryProductAudit(productId: string): Promise<{
  data: import('./data.d').ProductAuditEntry[];
  total: number;
  success: boolean;
}> {
  const params = new URLSearchParams({
    entityType: 'Product',
    entityId: productId,
    size: '100',
  });
  const data = await request<import('./data.d').ProductAuditEntry[]>(
    `/api/admin/audit?${params.toString()}`,
    { method: 'GET', ...adminOpts },
  );
  return { data: data ?? [], total: data?.length ?? 0, success: true };
}

export async function createProduct(body: CreateProductPayload) {
  return request('/api/admin/products', {
    method: 'POST',
    data: body,
    ...adminOpts,
  });
}

export async function updateProduct(id: string, body: UpdateProductPayload) {
  return request(`/api/admin/products/${id}`, {
    method: 'PUT',
    data: body,
    ...adminOpts,
  });
}

export async function deleteProduct(id: string) {
  return request(`/api/admin/products/${id}`, {
    method: 'DELETE',
    ...adminOpts,
  });
}
