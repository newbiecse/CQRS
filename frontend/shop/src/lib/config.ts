export function getGatewayUrl(): string {
  return process.env.GATEWAY_URL ?? "http://localhost:5000";
}

export function getProductApiBase(): string {
  return `${getGatewayUrl()}/product-queries/api`;
}
