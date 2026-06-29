"use client";

import { useCallback, useEffect, useState } from "react";
import {
  AdminShell,
  Button,
  Card,
  DataTable,
  ErrorMessage,
  SuccessMessage,
} from "@/components/ui";
import { AdminApiError, api } from "@/lib/api";
import type { Order } from "@/lib/types";

export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selected, setSelected] = useState<Order | null>(null);

  const loadOrders = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setOrders(await api.orders.list());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load orders");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadOrders();
  }, [loadOrders]);

  async function handleCancel(orderId: string) {
    setError(null);
    setSuccess(null);
    try {
      await api.orders.cancel(orderId);
      setSuccess(`Cancel requested for order ${orderId}`);
      await loadOrders();
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Cancel failed");
    }
  }

  async function handleSelect(orderId: string) {
    try {
      setSelected(await api.orders.get(orderId));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load order");
    }
  }

  return (
    <AdminShell title="Orders">
      <div className="space-y-6">
        <ErrorMessage message={error} />
        <SuccessMessage message={success} />

        <Card title={loading ? "Loading..." : `Orders (${orders.length})`}>
          <DataTable
            columns={["Order ID", "Customer", "Status", "Total", "Actions"]}
            rows={orders.map((order) => [
              <button
                key={order.id}
                type="button"
                className="text-left text-accent hover:underline"
                onClick={() => void handleSelect(order.id)}
              >
                {order.id.slice(0, 8)}…
              </button>,
              order.customerId.slice(0, 8) + "…",
              order.status,
              `$${order.totalAmount.toFixed(2)}`,
              order.status !== "Cancelled" ? (
                <Button variant="danger" onClick={() => void handleCancel(order.id)}>
                  Cancel
                </Button>
              ) : (
                "—"
              ),
            ])}
          />
        </Card>

        {selected ? (
          <Card title={`Order detail — ${selected.id}`}>
            <dl className="grid gap-2 text-sm sm:grid-cols-2">
              <div>
                <dt className="text-slate-400">Status</dt>
                <dd>{selected.status}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Cart</dt>
                <dd className="font-mono text-xs">{selected.cartId}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Payment</dt>
                <dd className="font-mono text-xs">{selected.paymentId ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Total</dt>
                <dd>${selected.totalAmount.toFixed(2)}</dd>
              </div>
            </dl>
            <div className="mt-4">
              <p className="mb-2 text-xs text-slate-400">Line items</p>
              <DataTable
                columns={["Product", "Qty", "Unit", "Subtotal"]}
                rows={selected.lines.map((line) => [
                  line.productName,
                  String(line.quantity),
                  `$${line.unitPrice.toFixed(2)}`,
                  `$${(line.unitPrice * line.quantity).toFixed(2)}`,
                ])}
              />
            </div>
          </Card>
        ) : null}
      </div>
    </AdminShell>
  );
}
