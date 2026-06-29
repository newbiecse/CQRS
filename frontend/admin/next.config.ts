import type { NextConfig } from "next";

const adminApiUrl = process.env.ADMIN_API_URL ?? "http://localhost:5100";

const nextConfig: NextConfig = {
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${adminApiUrl}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
