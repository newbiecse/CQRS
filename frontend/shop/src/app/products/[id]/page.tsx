import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { getProductById } from "@/lib/api";

type ProductDetailPageProps = {
  params: Promise<{ id: string }>;
};

export async function generateMetadata({
  params,
}: ProductDetailPageProps): Promise<Metadata> {
  const { id } = await params;

  try {
    const product = await getProductById(id);
    if (!product) return { title: "Product not found" };
    return {
      title: product.name,
      description: `${product.name} — $${product.price.toFixed(2)}`,
    };
  } catch {
    return { title: "Product" };
  }
}

export default async function ProductDetailPage({ params }: ProductDetailPageProps) {
  const { id } = await params;

  let product: Awaited<ReturnType<typeof getProductById>> = null;
  try {
    product = await getProductById(id);
  } catch {
    throw new Error("Failed to load product");
  }

  if (!product) notFound();

  return (
    <main className="container">
      <p>
        <Link href="/products">← Back to catalog</Link>
      </p>

      <article className="detail-card">
        <h1 className="page-title">{product.name}</h1>
        <p className="price" style={{ fontSize: "1.5rem" }}>
          ${product.price.toFixed(2)}
        </p>
        <dl className="muted" style={{ marginTop: "1.5rem" }}>
          <div>
            <dt>Product ID</dt>
            <dd>{product.id}</dd>
          </div>
          <div>
            <dt>Created</dt>
            <dd>{new Date(product.createdAt).toLocaleString()}</dd>
          </div>
          <div>
            <dt>Last updated</dt>
            <dd>{new Date(product.lastUpdatedAt).toLocaleString()}</dd>
          </div>
        </dl>
      </article>
    </main>
  );
}
