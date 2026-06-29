export type ProductItem = {
  id: string;
  name: string;
  price: number;
  createdAt: string;
  lastUpdatedAt: string;
};

export type ProductSearchResult = ProductItem & {
  highlightedName: string;
};
