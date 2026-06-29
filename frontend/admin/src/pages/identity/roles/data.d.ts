export type RoleItem = {
  id: string;
  name: string;
  description: string;
  isSystem: boolean;
  createdAt: string;
  permissions: string[];
};

export type CreateRolePayload = {
  name: string;
  description: string;
};

export type UpdateRolePayload = {
  description: string;
};
