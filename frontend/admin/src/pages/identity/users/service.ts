import { request } from '@umijs/max';
import type { CreateUserPayload, UpdateUserPayload, UserItem } from './data.d';

const adminOpts = { skipErrorHandler: true };

export async function queryUsers(): Promise<{
  data: UserItem[];
  total: number;
  success: boolean;
}> {
  const data = await request<UserItem[]>('/api/admin/users', {
    method: 'GET',
    ...adminOpts,
  });
  return { data: data ?? [], total: data?.length ?? 0, success: true };
}

export async function getUserById(id: string): Promise<UserItem> {
  return request<UserItem>(`/api/admin/users/${id}`, { method: 'GET', ...adminOpts });
}

export async function createUser(body: CreateUserPayload): Promise<void> {
  await request('/api/admin/users', { method: 'POST', data: body, ...adminOpts });
}

export async function updateUser(id: string, body: UpdateUserPayload): Promise<void> {
  await request(`/api/admin/users/${id}`, { method: 'PUT', data: body, ...adminOpts });
}

export async function deactivateUser(id: string): Promise<void> {
  await request(`/api/admin/users/${id}/deactivate`, { method: 'POST', ...adminOpts });
}
