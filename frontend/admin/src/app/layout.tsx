import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Shop Admin Portal",
  description: "Admin portal for CQRS distributed microservices demo",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
