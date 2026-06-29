# Shopping site (Next.js SSR)

Customer-facing storefront using **Next.js App Router** with **server-side rendering**.

Data is fetched on the server from `Shop.Gateway.Api` (`GATEWAY_URL`, default `http://localhost:5000`).

## Pages

| Route | Rendering |
|-------|-----------|
| `/` | SSR search (`?q=`) via Elasticsearch |
| `/products` | SSR product catalog |
| `/products/[id]` | SSR product detail + metadata |
| `/chat` | AI shopping assistant (streaming) |

Search uses a native `GET` form (no client-side fetch). Navigation triggers a new server render.

Chat uses client-side streaming to `/api/chat/completions` (proxied to Chat.Api).

## Dev

```bash
pnpm install
pnpm dev
```

Open http://localhost:3001

Ensure backend is running (`shop-gateway-api` on `:5000`, `product-queries` on `:5211`, Elasticsearch for search).

## Environment

| Variable | Default | Purpose |
|----------|---------|---------|
| `GATEWAY_URL` | `http://localhost:5000` | Server-side API base (required in Docker/K8s) |

## Production

```bash
pnpm build
pnpm start
```

`next.config.ts` uses `output: "standalone"` for container builds (`infra/dockerfiles/Dockerfile.nextjs`).
