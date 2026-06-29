import type { ProductItem } from "@/lib/types";
import { ProductCard } from "./ProductCard";

type ProductGridProps = {
  products: ProductItem[];
  highlightedById?: Record<string, string>;
  emptyMessage?: string;
};

export function ProductGrid({
  products,
  highlightedById,
  emptyMessage = "No products found.",
}: ProductGridProps) {
  if (products.length === 0) {
    return <p className="muted">{emptyMessage}</p>;
  }

  return (
    <ul className="card-grid">
      {products.map((product) => (
        <ProductCard
          key={product.id}
          product={product}
          highlightedName={highlightedById?.[product.id]}
        />
      ))}
    </ul>
  );
}
