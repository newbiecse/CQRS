import type { NextConfig } from "next";

const gatewayUrl = process.env.GATEWAY_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  output: "standalone",
  async rewrites() {
    return [
      {
        source: "/api/products/:path*",
        destination: `${gatewayUrl}/product-queries/api/products/:path*`,
      },
      {
        source: "/api/chat/:path*",
        destination: `${gatewayUrl}/chat-api/api/chat/:path*`,
      },
    ];
  },
};

export default nextConfig;
