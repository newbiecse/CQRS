"use client";

import { useCallback, useEffect, useState } from "react";
import {
  AdminShell,
  Button,
  Card,
  DataTable,
  ErrorMessage,
  Input,
  Label,
  SuccessMessage,
} from "@/components/ui";
import { AdminApiError, api } from "@/lib/api";
import type { Product } from "@/lib/types";

export default function ProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [name, setName] = useState("");
  const [price, setPrice] = useState("");
  const [priceEdits, setPriceEdits] = useState<Record<string, string>>({});

  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setProducts(await api.products.list());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load products");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadProducts();
  }, [loadProducts]);

  async function handleCreate(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    try {
      const result = await api.products.create(name, Number(price));
      setSuccess(`Product created (${result.id}). Read model updates asynchronously.`);
      setName("");
      setPrice("");
      await loadProducts();
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Create failed");
    }
  }

  async function handleUpdatePrice(productId: string) {
    setError(null);
    setSuccess(null);
    const newPrice = Number(priceEdits[productId]);
    try {
      await api.products.updatePrice(productId, newPrice);
      setSuccess(`Price updated for ${productId}`);
      await loadProducts();
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Update failed");
    }
  }

  return (
    <AdminShell title="Products">
      <div className="space-y-6">
        <ErrorMessage message={error} />
        <SuccessMessage message={success} />

        <Card title="Create product">
          <form onSubmit={handleCreate} className="grid gap-3 sm:grid-cols-3">
            <div>
              <Label>Name</Label>
              <Input value={name} onChange={(e) => setName(e.target.value)} required />
            </div>
            <div>
              <Label>Price</Label>
              <Input
                type="number"
                step="0.01"
                min="0"
                value={price}
                onChange={(e) => setPrice(e.target.value)}
                required
              />
            </div>
            <div className="flex items-end">
              <Button type="submit">Create</Button>
            </div>
          </form>
        </Card>

        <Card title={loading ? "Loading..." : `Catalog (${products.length})`}>
          <DataTable
            columns={["Name", "Price", "Updated", "Actions"]}
            rows={products.map((product) => [
              product.name,
              `$${product.price.toFixed(2)}`,
              new Date(product.lastUpdatedAt).toLocaleString(),
              <div key={product.id} className="flex items-center gap-2">
                <Input
                  type="number"
                  step="0.01"
                  className="w-24"
                  placeholder="New price"
                  value={priceEdits[product.id] ?? ""}
                  onChange={(e) =>
                    setPriceEdits((prev) => ({ ...prev, [product.id]: e.target.value }))
                  }
                />
                <Button variant="ghost" onClick={() => void handleUpdatePrice(product.id)}>
                  Update
                </Button>
              </div>,
            ])}
          />
        </Card>
      </div>
    </AdminShell>
  );
}
