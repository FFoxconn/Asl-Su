export interface Store {
  id: number;
  name: string;
  code: string;
  address: string | null;
  isActive: boolean;
}

export interface Category {
  id: number;
  name: string;
  parentCategoryId: number | null;
}

export interface Brand {
  id: number;
  name: string;
}
