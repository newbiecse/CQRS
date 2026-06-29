"use client";

import { useState } from "react";
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
import type { Cart, Product, User } from "@/lib/types";

export default function CartsPage() {
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [customerId, setCustomerId] = useState("");
  const [cartId, setCartId] = useState("");
  const [cart, setCart] = useState<Cart | null>(null);
  const [users, setUsers] = useState<User[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [selectedProductId, setSelectedProductId] = useState("");
  const [quantity, setQuantity] = useState("1");

  async function loadReferenceData() {
    try {
      const [userList, productList] = await Promise.all([
        api.users.list(),
        api.products.list(),
      ]);
      setUsers(userList);
      setProducts(productList);
      if (userList[0] && !customerId) setCustomerId(userList[0].id);
      if (productList[0] && !selectedProductId) setSelectedProductId(productList[0].id);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load reference data");
    }
  }

  async function handleCreateCart(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    try {
      const result = await api.carts.create(customerId);
      setCartId(result.id);
      setSuccess(`Cart created: ${result.id}`);
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Create cart failed");
    }
  }

  async function handleLoadCart() {
    setError(null);
    try {
      setCart(await api.carts.get(cartId));
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Cart not found");
      setCart(null);
    }
  }

  async function handleAddItem(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    const product = products.find((p) => p.id === selectedProductId);
    if (!product) return;
    try {
      await api.carts.addItem(
        cartId,
        product.id,
        product.name,
        product.price,
        Number(quantity),
      );
      setSuccess("Item added");
      await handleLoadCart();
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Add item failed");
    }
  }

  async function handleCheckout() {
    setError(null);
    setSuccess(null);
    try {
      const result = await api.carts.checkout(cartId);
      setSuccess(`Checkout accepted. Order: ${result.orderId}`);
      await handleLoadCart();
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Checkout failed");
    }
  }

  return (
    <AdminShell title="Carts">
      <div className="space-y-6">
        <ErrorMessage message={error} />
        <SuccessMessage message={success} />

        <Card title="Setup">
          <Button variant="ghost" onClick={() => void loadReferenceData()}>
            Load users &amp; products
          </Button>
        </Card>

        <Card title="Create cart">
          <form onSubmit={handleCreateCart} className="grid gap-3 sm:grid-cols-3">
            <div className="sm:col-span-2">
              <Label>Customer ID (user)</Label>
              <Input
                list="user-ids"
                value={customerId}
                onChange={(e) => setCustomerId(e.target.value)}
                required
              />
              <datalist id="user-ids">
                {users.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.displayName}
                  </option>
                ))}
              </datalist>
            </div>
            <div className="flex items-end">
              <Button type="submit">Create cart</Button>
            </div>
          </form>
        </Card>

        <Card title="View / manage cart">
          <div className="mb-4 flex flex-wrap gap-2">
            <Input
              className="max-w-md"
              placeholder="Cart ID"
              value={cartId}
              onChange={(e) => setCartId(e.target.value)}
            />
            <Button variant="ghost" onClick={() => void handleLoadCart()}>
              Load
            </Button>
            <Button onClick={() => void handleCheckout()} disabled={!cartId}>
              Checkout (direct)
            </Button>
          </div>

          {cart ? (
            <>
              <dl className="mb-4 grid gap-2 text-sm sm:grid-cols-3">
                <div>
                  <dt className="text-slate-400">Status</dt>
                  <dd>{cart.status}</dd>
                </div>
                <div>
                  <dt className="text-slate-400">Subtotal</dt>
                  <dd>${cart.subtotal.toFixed(2)}</dd>
                </div>
                <div>
                  <dt className="text-slate-400">Customer</dt>
                  <dd className="font-mono text-xs">{cart.customerId}</dd>
                </div>
              </dl>
              <DataTable
                columns={["Product", "Qty", "Unit price", "Line total"]}
                rows={cart.items.map((item) => [
                  item.productName,
                  String(item.quantity),
                  `$${item.unitPrice.toFixed(2)}`,
                  `$${(item.unitPrice * item.quantity).toFixed(2)}`,
                ])}
              />
            </>
          ) : null}

          <form onSubmit={handleAddItem} className="mt-4 grid gap-3 sm:grid-cols-4">
            <div className="sm:col-span-2">
              <Label>Product</Label>
              <select
                className="w-full rounded-lg border border-surface-border bg-surface px-3 py-2 text-sm"
                value={selectedProductId}
                onChange={(e) => setSelectedProductId(e.target.value)}
              >
                {products.map((product) => (
                  <option key={product.id} value={product.id}>
                    {product.name} (${product.price})
                  </option>
                ))}
              </select>
            </div>
            <div>
              <Label>Quantity</Label>
              <Input
                type="number"
                min="1"
                value={quantity}
                onChange={(e) => setQuantity(e.target.value)}
              />
            </div>
            <div className="flex items-end">
              <Button type="submit" disabled={!cartId}>
                Add item
              </Button>
            </div>
          </form>
        </Card>
      </div>
    </AdminShell>
  );
}
