# Shopping site

Customer-facing storefront (Next.js) calling `Shop.Gateway.Api`.

## Dev

```bash
pnpm install
pnpm dev
```

Open http://localhost:3001

Product search uses Elasticsearch via:

`GET /product-queries/api/products/search?q=phone`

Rewritten in Next.js to the gateway (`:5000`).
