"use client";

import { useState } from "react";
import {
  AdminShell,
  Button,
  Card,
  ErrorMessage,
  Input,
  Label,
  SuccessMessage,
} from "@/components/ui";
import { AdminApiError, api } from "@/lib/api";
import type { CheckoutSaga } from "@/lib/types";

export default function SagaPage() {
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [cartId, setCartId] = useState("");
  const [sagaId, setSagaId] = useState("");
  const [simulateFailure, setSimulateFailure] = useState(false);
  const [saga, setSaga] = useState<CheckoutSaga | null>(null);
  const [polling, setPolling] = useState(false);

  async function handleStartCheckout(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    try {
      const result = await api.saga.startCheckout(cartId, simulateFailure);
      setSagaId(result.sagaId);
      setSuccess(`Saga started: ${result.sagaId}`);
      await loadSaga(result.sagaId);
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Start saga failed");
    }
  }

  async function loadSaga(id = sagaId) {
    if (!id) return;
    setError(null);
    try {
      setSaga(await api.saga.get(id));
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Saga not found");
      setSaga(null);
    }
  }

  async function handlePoll() {
    if (!sagaId) return;
    setPolling(true);
    setError(null);
    try {
      for (let attempt = 0; attempt < 15; attempt += 1) {
        const current = await api.saga.get(sagaId);
        setSaga(current);
        if (
          current.state === "Completed" ||
          current.state === "Compensated" ||
          current.state === "Failed"
        ) {
          break;
        }
        await new Promise((resolve) => setTimeout(resolve, 2000));
      }
    } catch (e) {
      setError(e instanceof Error ? e.message : "Poll failed");
    } finally {
      setPolling(false);
    }
  }

  return (
    <AdminShell title="Checkout Saga">
      <div className="space-y-6">
        <ErrorMessage message={error} />
        <SuccessMessage message={success} />

        <Card title="Start checkout saga">
          <p className="mb-4 text-sm text-slate-400">
            Orchestrates cart checkout → order → payment → mark paid (or compensate on failure).
          </p>
          <form onSubmit={handleStartCheckout} className="space-y-3">
            <div>
              <Label>Cart ID</Label>
              <Input value={cartId} onChange={(e) => setCartId(e.target.value)} required />
            </div>
            <label className="flex items-center gap-2 text-sm text-slate-300">
              <input
                type="checkbox"
                checked={simulateFailure}
                onChange={(e) => setSimulateFailure(e.target.checked)}
              />
              Simulate payment failure (compensation demo)
            </label>
            <Button type="submit">Start saga</Button>
          </form>
        </Card>

        <Card title="Saga status">
          <div className="mb-4 flex flex-wrap gap-2">
            <Input
              className="max-w-md"
              placeholder="Saga ID"
              value={sagaId}
              onChange={(e) => setSagaId(e.target.value)}
            />
            <Button variant="ghost" onClick={() => void loadSaga()}>
              Load
            </Button>
            <Button onClick={() => void handlePoll()} disabled={!sagaId || polling}>
              {polling ? "Polling…" : "Poll until terminal"}
            </Button>
          </div>

          {saga ? (
            <dl className="grid gap-3 text-sm sm:grid-cols-2">
              <div>
                <dt className="text-slate-400">State</dt>
                <dd className="text-lg font-medium text-white">{saga.state}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Cart</dt>
                <dd className="font-mono text-xs">{saga.cartId}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Order</dt>
                <dd className="font-mono text-xs">{saga.orderId ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Payment</dt>
                <dd className="font-mono text-xs">{saga.paymentId ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Failure reason</dt>
                <dd>{saga.failureReason ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-slate-400">Updated</dt>
                <dd>{new Date(saga.updatedAt).toLocaleString()}</dd>
              </div>
            </dl>
          ) : (
            <p className="text-sm text-slate-500">Enter a saga ID to inspect orchestration state.</p>
          )}
        </Card>
      </div>
    </AdminShell>
  );
}
