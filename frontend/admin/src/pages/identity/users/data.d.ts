export type UserItem = {
  id: string;
  email: string;
  displayName: string;
  isActive: boolean;
  createdAt: string;
  roles: string[];
  permissions: string[];
};

export type CreateUserPayload = {
  email: string;
  displayName: string;
  password: string;
  roles?: string[];
};

export type UpdateUserPayload = {
  displayName: string;
  roles: string[];
  isActive: boolean;
  password?: string;
};
