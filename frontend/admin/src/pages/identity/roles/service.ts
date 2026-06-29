import { request } from '@umijs/max';
import type { CreateRolePayload, RoleItem, UpdateRolePayload } from './data.d';

const adminOpts = { skipErrorHandler: true };

export async function queryRoles(): Promise<{
  data: RoleItem[];
  total: number;
  success: boolean;
}> {
  const data = await request<RoleItem[]>('/api/admin/roles', {
    method: 'GET',
    ...adminOpts,
  });
  return { data: data ?? [], total: data?.length ?? 0, success: true };
}

export async function createRole(body: CreateRolePayload): Promise<void> {
  await request('/api/admin/roles', { method: 'POST', data: body, ...adminOpts });
}

export async function updateRole(id: string, body: UpdateRolePayload): Promise<void> {
  await request(`/api/admin/roles/${id}`, { method: 'PUT', data: body, ...adminOpts });
}

export async function deleteRole(id: string): Promise<void> {
  await request(`/api/admin/roles/${id}`, { method: 'DELETE', ...adminOpts });
}

export async function setRolePermissions(id: string, permissions: string[]): Promise<void> {
  await request(`/api/admin/roles/${id}/permissions`, {
    method: 'PUT',
    data: { permissions },
    ...adminOpts,
  });
}
