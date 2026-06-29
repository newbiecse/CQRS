"use client";

import { FormEvent, useState } from "react";
import { searchProducts } from "@/lib/api";
import type { ProductSearchResult } from "@/lib/types";

export function ProductSearch() {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<ProductSearchResult[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = query.trim();
    if (!trimmed) return;

    setLoading(true);
    setError(null);

    try {
      setResults(await searchProducts(trimmed));
    } catch (err) {
      setResults([]);
      setError(err instanceof Error ? err.message : "Search failed");
    } finally {
      setLoading(false);
    }
  }

  return (
    <section>
      <form onSubmit={onSubmit} style={{ display: "flex", gap: "0.75rem" }}>
        <input
          type="search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Search products by name..."
          aria-label="Search products"
          style={{
            flex: 1,
            padding: "0.75rem 1rem",
            borderRadius: "8px",
            border: "1px solid #d0d7de",
            fontSize: "1rem",
          }}
        />
        <button
          type="submit"
          disabled={loading || !query.trim()}
          style={{
            padding: "0.75rem 1.25rem",
            borderRadius: "8px",
            border: "none",
            background: "#0969da",
            color: "#fff",
            fontWeight: 600,
            cursor: "pointer",
          }}
        >
          {loading ? "Searching..." : "Search"}
        </button>
      </form>

      {error ? (
        <p role="alert" style={{ color: "#cf222e", marginTop: "1rem" }}>
          {error}
        </p>
      ) : null}

      <ul style={{ listStyle: "none", padding: 0, marginTop: "1.5rem" }}>
        {results.map((product) => (
          <li
            key={product.id}
            style={{
              padding: "1rem",
              border: "1px solid #d0d7de",
              borderRadius: "8px",
              marginBottom: "0.75rem",
            }}
          >
            <h2
              style={{ margin: 0, fontSize: "1.1rem" }}
              dangerouslySetInnerHTML={{ __html: product.highlightedName }}
            />
            <p style={{ margin: "0.5rem 0 0", color: "#57606a" }}>
              ${product.price.toFixed(2)}
            </p>
          </li>
        ))}
      </ul>

      {!loading && !error && query.trim() && results.length === 0 ? (
        <p style={{ marginTop: "1rem", color: "#57606a" }}>No products found.</p>
      ) : null}
    </section>
  );
}
