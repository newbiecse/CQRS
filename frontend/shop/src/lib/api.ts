import { getProductApiBase } from "./config";
import type { ProductItem, ProductSearchResult } from "./types";

async function fetchJson<T>(
  url: string,
  options?: { revalidate?: number | false },
): Promise<T> {
  const response = await fetch(url, {
    headers: { Accept: "application/json" },
    ...(options?.revalidate === false
      ? { cache: "no-store" as const }
      : { next: { revalidate: options?.revalidate ?? 30 } }),
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed (${response.status})`);
  }

  return response.json() as Promise<T>;
}

export async function getProducts(): Promise<ProductItem[]> {
  return fetchJson<ProductItem[]>(`${getProductApiBase()}/products`);
}

export async function getProductById(id: string): Promise<ProductItem | null> {
  const response = await fetch(`${getProductApiBase()}/products/${id}`, {
    headers: { Accept: "application/json" },
    next: { revalidate: 30 },
  });

  if (response.status === 404) return null;
  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || `Request failed (${response.status})`);
  }

  return response.json() as Promise<ProductItem>;
}

export async function searchProducts(
  query: string,
  size = 20,
): Promise<ProductSearchResult[]> {
  const params = new URLSearchParams({ q: query, size: String(size) });
  return fetchJson<ProductSearchResult[]>(
    `${getProductApiBase()}/products/search?${params.toString()}`,
    { revalidate: false },
  );
}
