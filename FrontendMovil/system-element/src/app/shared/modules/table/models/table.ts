export interface TableData<T> {
  item: T;
  id: string;
}

export type TableTemplateType =
  | 'header'
  | 'footer'
  | 'body'
  | 'pager'
  | 'itemsChange'
  | 'loading'
  | 'noItemsFound';
