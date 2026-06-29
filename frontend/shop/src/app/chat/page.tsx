import type { Metadata } from "next";
import { ShopChat } from "@/components/ShopChat";

export const metadata: Metadata = {
  title: "Chat assistant",
  description: "Ask the CQRS Shop AI assistant about products",
};

export default function ChatPage() {
  return (
    <main className="container">
      <h1 className="page-title">Shop assistant</h1>
      <p className="page-subtitle">
        Chat with our AI agent. Answers use the live product catalog.
      </p>
      <ShopChat />
    </main>
  );
}
