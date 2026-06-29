export type PermissionItem = {
  id: string;
  name: string;
  description: string;
  createdAt: string;
};

export type CreatePermissionPayload = {
  name: string;
  description: string;
};

export type UpdatePermissionPayload = {
  description: string;
};
