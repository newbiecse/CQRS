import { request } from '@umijs/max';
import type { ReportPeriod, TopBuyerItem, TopProductItem } from './data.d';

const adminOpts = { skipErrorHandler: true };

function buildQuery(limit: number, date?: string) {
  const params = new URLSearchParams();
  params.set('limit', String(limit));
  if (date) params.set('date', date);
  return `?${params.toString()}`;
}

export async function queryTopBuyers(
  period: ReportPeriod,
  limit = 10,
  date?: string,
): Promise<TopBuyerItem[]> {
  return request<TopBuyerItem[]>(
    `/api/admin/reports/top-users/${period}${buildQuery(limit, date)}`,
    { method: 'GET', ...adminOpts },
  );
}

export async function queryTopProducts(
  period: ReportPeriod,
  limit = 10,
  date?: string,
): Promise<TopProductItem[]> {
  return request<TopProductItem[]>(
    `/api/admin/reports/top-products/${period}${buildQuery(limit, date)}`,
    { method: 'GET', ...adminOpts },
  );
}
