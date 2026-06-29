import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "CQRS Shop",
  description: "Search products by name",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
      <style>{`mark { background: #fff8c5; padding: 0 0.15em; border-radius: 2px; }`}</style>
    </html>
  );
}
