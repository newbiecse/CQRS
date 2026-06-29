export type ProductItem = {
  id: string;
  name: string;
  price: number;
  createdAt: string;
  lastUpdatedAt: string;
};

export type CreateProductPayload = {
  name: string;
  price: number;
};

export type UpdateProductPayload = {
  name: string;
  price: number;
};

export type ProductAuditEntry = {
  id: string;
  occurredAtUtc: string;
  eventType: string;
  entityType: string;
  entityId: string;
  action: string;
  service: string;
  summary: string;
  payloadJson: string;
};
