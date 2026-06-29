import { AdminShell, Card, DataTable } from "@/components/ui";
import { api } from "@/lib/api";

export const dynamic = "force-dynamic";

export default async function DashboardPage() {
  let summary: Awaited<ReturnType<typeof api.dashboard>> | null = null;
  let error: string | null = null;

  try {
    summary = await api.dashboard();
  } catch (e) {
    error = e instanceof Error ? e.message : "Failed to load dashboard";
  }

  const topUsers = summary?.topUsers ?? [];

  return (
    <AdminShell title="Dashboard">
      {error ? (
        <p className="mb-4 rounded-lg border border-amber-800 bg-amber-950/40 px-3 py-2 text-sm text-amber-200">
          {error} — ensure Shop.Admin.Api and backend services are running.
        </p>
      ) : null}

      <div className="mb-6 grid gap-4 sm:grid-cols-3">
        <Card>
          <p className="text-2xl font-semibold">{summary?.productCount ?? "—"}</p>
          <p className="text-sm text-slate-400">Products</p>
        </Card>
        <Card>
          <p className="text-2xl font-semibold">{summary?.orderCount ?? "—"}</p>
          <p className="text-sm text-slate-400">Orders</p>
        </Card>
        <Card>
          <p className="text-2xl font-semibold">{topUsers.length}</p>
          <p className="text-sm text-slate-400">Top users (week)</p>
        </Card>
      </div>

      <Card title="Top users by order amount (this week)">
        <DataTable
          columns={["User", "Email", "Orders", "Total"]}
          rows={topUsers.map((user) => [
            user.displayName,
            user.email,
            String(user.orderCount),
            `$${user.totalOrderAmount.toFixed(2)}`,
          ])}
        />
      </Card>
    </AdminShell>
  );
}
