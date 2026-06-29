import type { ApiError } from "./types";

function resolveBaseUrl(): string {
  if (typeof window === "undefined") {
    return process.env.ADMIN_API_URL ?? "http://localhost:5100";
  }
  return "/api";
}

export class AdminApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
  ) {
    super(message);
    this.name = "AdminApiError";
  }
}

export async function adminFetch<T>(
  path: string,
  init?: RequestInit,
): Promise<T> {
  const url = `${resolveBaseUrl()}${path.startsWith("/") ? path : `/${path}`}`;
  const response = await fetch(url, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...init?.headers,
    },
    cache: "no-store",
  });

  if (!response.ok) {
    let message = response.statusText;
    try {
      const body = (await response.json()) as ApiError;
      if (body.message) message = body.message;
    } catch {
      // ignore parse errors
    }
    throw new AdminApiError(message, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export type DashboardSummary = {
  productCount: number;
  orderCount: number;
  topUsers: import("./types").TopUserReport[];
};

export const api = {
  dashboard: () => adminFetch<DashboardSummary>("/api/admin/dashboard"),
  products: {
    list: () => adminFetch<import("./types").Product[]>("/api/admin/products"),
    get: (id: string) =>
      adminFetch<import("./types").Product>(`/api/admin/products/${id}`),
    create: (name: string, price: number) =>
      adminFetch<{ id: string }>("/api/admin/products", {
        method: "POST",
        body: JSON.stringify({ name, price }),
      }),
    updatePrice: (id: string, newPrice: number) =>
      adminFetch<{ id: string }>(`/api/admin/products/${id}/price`, {
        method: "PUT",
        body: JSON.stringify({ newPrice }),
      }),
  },
  users: {
    list: () => adminFetch<import("./types").User[]>("/api/admin/users"),
    create: (email: string, displayName: string) =>
      adminFetch<{ userId: string }>("/api/admin/users", {
        method: "POST",
        body: JSON.stringify({ email, displayName }),
      }),
    deactivate: (userId: string) =>
      adminFetch<void>(`/api/admin/users/${userId}/deactivate`, {
        method: "POST",
        body: JSON.stringify({}),
      }),
  },
  orders: {
    list: () => adminFetch<import("./types").Order[]>("/api/admin/orders"),
    get: (id: string) =>
      adminFetch<import("./types").Order>(`/api/admin/orders/${id}`),
    cancel: (orderId: string, reason = "Cancelled from admin portal") =>
      adminFetch<void>(`/api/admin/orders/${orderId}/cancel`, {
        method: "POST",
        body: JSON.stringify({ reason }),
      }),
  },
  carts: {
    get: (id: string) =>
      adminFetch<import("./types").Cart>(`/api/admin/carts/${id}`),
    create: (customerId: string) =>
      adminFetch<{ id: string }>("/api/admin/carts", {
        method: "POST",
        body: JSON.stringify({ customerId }),
      }),
    addItem: (
      cartId: string,
      productId: string,
      productName: string,
      unitPrice: number,
      quantity: number,
    ) =>
      adminFetch<{ cartId: string }>(`/api/admin/carts/${cartId}/items`, {
        method: "POST",
        body: JSON.stringify({ productId, productName, unitPrice, quantity }),
      }),
    checkout: (cartId: string) =>
      adminFetch<{ orderId: string; cartId: string }>(
        `/api/admin/carts/${cartId}/checkout`,
        { method: "POST", body: JSON.stringify({}) },
      ),
  },
  reporting: {
    topUsers: (period: "day" | "week" | "month", limit = 10) =>
      adminFetch<import("./types").TopUserReport[]>(
        `/api/admin/reports/top-users/${period}?limit=${limit}`,
      ),
  },
  saga: {
    get: (sagaId: string) =>
      adminFetch<import("./types").CheckoutSaga>(`/api/admin/sagas/${sagaId}`),
    startCheckout: (cartId: string, simulatePaymentFailure = false) =>
      adminFetch<{ sagaId: string }>("/api/admin/sagas/checkout", {
        method: "POST",
        body: JSON.stringify({ cartId, simulatePaymentFailure }),
      }),
  },
};
