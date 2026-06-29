import { ProductSearch } from "@/components/ProductSearch";

export default function HomePage() {
  return (
    <main
      style={{
        maxWidth: "720px",
        margin: "0 auto",
        padding: "3rem 1.5rem",
        fontFamily: "system-ui, sans-serif",
      }}
    >
      <h1 style={{ marginTop: 0 }}>CQRS Shop</h1>
      <p style={{ color: "#57606a", marginBottom: "2rem" }}>
        Find products by name. Matches are highlighted in results.
      </p>
      <ProductSearch />
    </main>
  );
}
