import { request } from '@umijs/max';
import type {
  CreatePermissionPayload,
  PermissionItem,
  UpdatePermissionPayload,
} from './data.d';

const adminOpts = { skipErrorHandler: true };

export async function queryPermissions(): Promise<{
  data: PermissionItem[];
  total: number;
  success: boolean;
}> {
  const data = await request<PermissionItem[]>('/api/admin/permissions', {
    method: 'GET',
    ...adminOpts,
  });
  return { data: data ?? [], total: data?.length ?? 0, success: true };
}

export async function createPermission(body: CreatePermissionPayload): Promise<void> {
  await request('/api/admin/permissions', { method: 'POST', data: body, ...adminOpts });
}

export async function updatePermission(
  id: string,
  body: UpdatePermissionPayload,
): Promise<void> {
  await request(`/api/admin/permissions/${id}`, { method: 'PUT', data: body, ...adminOpts });
}

export async function deletePermission(id: string): Promise<void> {
  await request(`/api/admin/permissions/${id}`, { method: 'DELETE', ...adminOpts });
}
