import type { Metadata } from "next";
import { ProductGrid } from "@/components/ProductGrid";
import { getProducts } from "@/lib/api";

export const metadata: Metadata = {
  title: "All products",
};

export default async function ProductsPage() {
  let error: string | null = null;
  let products: Awaited<ReturnType<typeof getProducts>> = [];

  try {
    products = await getProducts();
  } catch (err) {
    error = err instanceof Error ? err.message : "Failed to load products";
  }

  return (
    <main className="container">
      <h1 className="page-title">All products</h1>
      <p className="page-subtitle">
        Catalog loaded on the server from Product Queries API.
      </p>

      {error ? <p className="alert">{error}</p> : null}

      {!error ? (
        <ProductGrid
          products={products}
          emptyMessage="No products in the catalog yet."
        />
      ) : null}
    </main>
  );
}
