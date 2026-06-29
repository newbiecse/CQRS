import { ProductGrid } from "@/components/ProductGrid";
import { SearchForm } from "@/components/SearchForm";
import { searchProducts } from "@/lib/api";

type HomePageProps = {
  searchParams: Promise<{ q?: string }>;
};

export default async function HomePage({ searchParams }: HomePageProps) {
  const { q } = await searchParams;
  const query = q?.trim() ?? "";

  let error: string | null = null;
  let results: Awaited<ReturnType<typeof searchProducts>> = [];

  if (query) {
    try {
      results = await searchProducts(query);
    } catch (err) {
      error = err instanceof Error ? err.message : "Search failed";
    }
  }

  const highlightedById = Object.fromEntries(
    results.map((item) => [item.id, item.highlightedName]),
  );

  return (
    <main className="container">
      <h1 className="page-title">Search products</h1>
      <p className="page-subtitle">
        Server-rendered search via Elasticsearch. Results update on each navigation.
      </p>

      <SearchForm defaultQuery={query} />

      {error ? <p className="alert">{error}</p> : null}

      {query && !error ? (
        <>
          <p className="muted">
            {results.length} result{results.length === 1 ? "" : "s"} for &quot;{query}&quot;
          </p>
          <ProductGrid
            products={results}
            highlightedById={highlightedById}
            emptyMessage={`No products found for "${query}".`}
          />
        </>
      ) : null}
    </main>
  );
}
