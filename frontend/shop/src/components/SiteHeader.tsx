import Link from "next/link";

export function SiteHeader() {
  return (
    <header className="site-header">
      <div className="site-header-inner">
        <Link href="/" className="brand">
          CQRS Shop
        </Link>
        <nav className="site-nav" aria-label="Main">
          <Link href="/">Search</Link>
          <Link href="/products">All products</Link>
        </nav>
      </div>
    </header>
  );
}
