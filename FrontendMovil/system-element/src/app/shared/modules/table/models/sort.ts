export enum Sort {
  Ascendant = 'ASC',
  Descendant = 'DESC',
  None = 'NONE',
  Disabled = 'DISABLED',
}

export interface SortContext {
  field: string;
  order: Sort;
}
