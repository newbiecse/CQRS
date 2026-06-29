import type { ProductSearchResult } from "./types";

export async function searchProducts(
  query: string,
  size = 20,
): Promise<ProductSearchResult[]> {
  const params = new URLSearchParams({ q: query, size: String(size) });
  const response = await fetch(`/api/products/search?${params.toString()}`, {
    cache: "no-store",
  });

  if (!response.ok) {
    const message = await response.text();
    throw new Error(message || "Product search failed");
  }

  return response.json();
}
