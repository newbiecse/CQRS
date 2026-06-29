import Link from "next/link";

export default function ProductNotFound() {
  return (
    <main className="container">
      <h1 className="page-title">Product not found</h1>
      <p className="page-subtitle">This product does not exist or was removed.</p>
      <Link href="/products" className="button">
        Browse catalog
      </Link>
    </main>
  );
}
