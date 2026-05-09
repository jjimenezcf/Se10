export interface PaginationContext {
  id?: string;
  itemsPerPage: number;
  currentPage: number;
  totalItems?: number;
}

export interface Page {
  label: string;
  value: any;
}

export type Collection<T> = T[];

export interface PaginatePipeArgs {
  id?: string;
  itemsPerPage?: string | number;
  currentPage?: string | number;
  totalItems?: string | number;
}

export interface PipeState {
  collection: any[];
  size: number;
  start: number;
  end: number;
  slice: any[];
}
