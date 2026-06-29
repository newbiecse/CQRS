import Link from "next/link";
import type { ProductItem } from "@/lib/types";

type ProductCardProps = {
  product: ProductItem;
  highlightedName?: string;
};

export function ProductCard({ product, highlightedName }: ProductCardProps) {
  return (
    <li className="product-card">
      <h2>
        {highlightedName ? (
          <Link
            href={`/products/${product.id}`}
            dangerouslySetInnerHTML={{ __html: highlightedName }}
          />
        ) : (
          <Link href={`/products/${product.id}`}>{product.name}</Link>
        )}
      </h2>
      <p className="price">${product.price.toFixed(2)}</p>
    </li>
  );
}
