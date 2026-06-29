/**
 * @see https://umijs.org/docs/max/access#access
 * */
export default function access(
  initialState: { currentUser?: API.CurrentUser } | undefined,
) {
  const { currentUser } = initialState ?? {};
  const roles: string[] =
    (currentUser as API.CurrentUser & { roles?: string[] })?.roles ??
    (currentUser?.access ? [currentUser.access] : []);

  const hasRole = (...allowed: string[]) =>
    allowed.some((role) => roles.includes(role));

  return {
    canAdmin: hasRole('admin'),
    canManageCatalog: hasRole('admin', 'catalog-manager'),
    canManageOrders: hasRole('admin', 'order-manager'),
  };
}
