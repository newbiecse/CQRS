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
import type { User } from "@/lib/types";

export default function UsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [email, setEmail] = useState("");
  const [displayName, setDisplayName] = useState("");

  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setUsers(await api.users.list());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load users");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadUsers();
  }, [loadUsers]);

  async function handleRegister(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    try {
      const result = await api.users.create(email, displayName);
      setSuccess(`User registered: ${result.userId}`);
      setEmail("");
      setDisplayName("");
      await loadUsers();
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Register failed");
    }
  }

  async function handleDeactivate(userId: string) {
    setError(null);
    setSuccess(null);
    try {
      await api.users.deactivate(userId);
      setSuccess(`User ${userId} deactivated`);
      await loadUsers();
    } catch (e) {
      setError(e instanceof AdminApiError ? e.message : "Deactivate failed");
    }
  }

  return (
    <AdminShell title="Users">
      <div className="space-y-6">
        <ErrorMessage message={error} />
        <SuccessMessage message={success} />

        <Card title="Register user">
          <form onSubmit={handleRegister} className="grid gap-3 sm:grid-cols-3">
            <div>
              <Label>Email</Label>
              <Input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div>
              <Label>Display name</Label>
              <Input
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                required
              />
            </div>
            <div className="flex items-end">
              <Button type="submit">Register</Button>
            </div>
          </form>
        </Card>

        <Card title={loading ? "Loading..." : `Users (${users.length})`}>
          <DataTable
            columns={["Name", "Email", "Status", "Registered", "Actions"]}
            rows={users.map((user) => [
              user.displayName,
              user.email,
              user.isActive ? (
                <span className="text-emerald-400">Active</span>
              ) : (
                <span className="text-slate-500">Inactive</span>
              ),
              new Date(user.registeredAt).toLocaleString(),
              user.isActive ? (
                <Button variant="danger" onClick={() => void handleDeactivate(user.id)}>
                  Deactivate
                </Button>
              ) : (
                "—"
              ),
            ])}
          />
        </Card>
      </div>
    </AdminShell>
  );
}
