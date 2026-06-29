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
      },
      {
        path: '/catalog/products/:id',
        name: 'product-detail',
        component: './catalog/products/detail',
        hideInMenu: true,
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
