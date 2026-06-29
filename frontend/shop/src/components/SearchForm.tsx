type SearchFormProps = {
  defaultQuery?: string;
};

export function SearchForm({ defaultQuery = "" }: SearchFormProps) {
  return (
    <form className="search-form" action="/" method="get">
      <input
        type="search"
        name="q"
        defaultValue={defaultQuery}
        placeholder="Search products by name..."
        aria-label="Search products"
        required
      />
      <button type="submit">Search</button>
    </form>
  );
}
