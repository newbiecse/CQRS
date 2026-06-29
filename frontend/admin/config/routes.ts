/**
 * CQRS Shop Admin — focused routes (demo boilerplate removed).
 */
export default [
  {
    path: '/user',
    layout: false,
    routes: [
      { path: '/user/login', name: 'login', component: './user/login' },
      { path: '/user', redirect: '/user/login' },
      { name: '404', component: './exception/404', path: '/user/*' },
    ],
  },
  {
    path: '/welcome',
    name: 'welcome',
    icon: 'home',
    component: './Welcome',
  },
  {
    path: '/catalog',
    name: 'catalog',
    icon: 'shop',
    routes: [
      { path: '/catalog', redirect: '/catalog/products' },
      {
        path: '/catalog/products',
        name: 'products',
        icon: 'product',
        component: './catalog/products',
        access: 'canManageCatalog',
      },
      {
        path: '/catalog/products/:id',
        name: 'product-detail',
        component: './catalog/products/detail',
        hideInMenu: true,
        access: 'canManageCatalog',
      },
    ],
  },
  {
    path: '/orders',
    name: 'orders',
    icon: 'shopping',
    routes: [
      { path: '/orders', redirect: '/orders/list' },
      {
        path: '/orders/list',
        name: 'order-list',
        component: './orders',
        access: 'canManageOrders',
      },
      {
        path: '/orders/:id',
        name: 'order-detail',
        component: './orders/detail',
        hideInMenu: true,
        access: 'canManageOrders',
      },
    ],
  },
  {
    path: '/identity',
    name: 'identity',
    icon: 'team',
    access: 'canAdmin',
    routes: [
      { path: '/identity', redirect: '/identity/users' },
      {
        path: '/identity/users',
        name: 'users',
        component: './identity/users',
        access: 'canAdmin',
      },
      {
        path: '/identity/roles',
        name: 'roles',
        component: './identity/roles',
        access: 'canAdmin',
      },
      {
        path: '/identity/permissions',
        name: 'permissions',
        component: './identity/permissions',
        access: 'canAdmin',
      },
    ],
  },
  {
    path: '/',
    redirect: '/catalog/products',
  },
  {
    component: './exception/404',
    path: '/*',
  },
];
