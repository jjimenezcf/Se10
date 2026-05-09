import {
  AfterContentInit,
  Component,
  ContentChildren,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  QueryList,
  SimpleChanges,
  TemplateRef,
  ViewEncapsulation,
} from '@angular/core';
import { TableTemplateDirective } from '../../directives/table-template.directive';
import { Sort, SortContext } from '../../models/sort';
import { TableService } from '../../services/table.service';
import { TableData } from '../../models/table';
import { Guid } from 'src/app/utils/guid';

@Component({
  selector: 'app-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss'],
  encapsulation: ViewEncapsulation.None,
  providers: [TableService],
})
export class TableComponent<T> implements AfterContentInit, OnChanges {
  @Input() set items(items: T[]) {
    this.tableItems = this.buildTableItems(items);
  }
  @Input() set id(id: string) {
    this.tableId = id ?? Guid.newGuid();
  }

  /* Items */
  @Input() bindIdentifier = 'id';

  /* Pagination */
  @Input() pagination: boolean;
  @Input() totalCount: number;
  @Input() itemsPerPage: number;
  @Input() currentPage: number;
  @Input() pagerAutoHide: boolean;
  @Input() pagerMaxSize = 10;

  /* Styles */
  @Input() class: string;
  @Input() style: any;
  @Input() tableStyle: any;
  @Input() tableClass: string;
  @Input() pagerContainerStyle: any;
  @Input() pagerContainerClass: any;
  @Input() elementNumberContainerStyle: any;
  @Input() elementNumberContainerClass: any;

  /* Items count */
  @Input() elementNumber: boolean;
  @Input() elementNumberAutoHide: boolean;
  @Input() elementNumberOptions: number[] = [5, 30, 40, 50];

  /* Sorting */
  @Input() sorting: boolean;
  @Input() sortContext: SortContext[] = [];

  /* Loading */
  @Input() loading: boolean;
  @Input() iterateLoading: boolean;

  /* Events */
  @Output() onPage = new EventEmitter<number>();
  @Output() onElementsNumber = new EventEmitter<number>();
  @Output() onSort = new EventEmitter<SortContext>();

  @ContentChildren(TableTemplateDirective)
  templates: QueryList<TableTemplateDirective>;

  /* Templates */
  headerTemplate: TemplateRef<any>;
  bodyTemplate: TemplateRef<any>;
  footerTemplate: TemplateRef<any>;
  noItemsFoundTemplate: TemplateRef<any>;
  pagerTemplate: TemplateRef<any>;
  itemsNumberTemplate: TemplateRef<any>;
  loadingTemplate: TemplateRef<any>;

  tableId: string;
  tableItems: TableData<T>[] = [];

  protected itemsPerPageLoadingIterate: number[] = [];
  private _storedTableItemsIds: { [item: string]: string } = {};

  constructor(public tableService: TableService) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (this.iterateLoading && changes['itemsPerPage']) {
      this.itemsPerPageLoadingIterate = Array(this.itemsPerPage);
    }
  }

  ngAfterContentInit(): void {
    this.templates.forEach((template) => {
      switch (template.getType()) {
        case 'body':
          this.bodyTemplate = template.template;
          break;
        case 'header':
          this.headerTemplate = template.template;
          break;
        case 'footer':
          this.footerTemplate = template.template;
          break;
        case 'pager':
          this.pagerTemplate = template.template;
          break;
        case 'itemsChange':
          this.itemsNumberTemplate = template.template;
          break;
        case 'noItemsFound':
          this.noItemsFoundTemplate = template.template;
          break;
        case 'loading':
          this.loadingTemplate = template.template;
          break;
      }
    });
  }

  isSorted(field: string) {
    if (!this.sorting || !this.sortContext?.length) {
      return false;
    }

    const order = this.findSortContext(field)?.order;
    return order !== Sort.None;
  }

  findSortContext(field: string): SortContext {
    if (!this.sorting || !this.sortContext?.length) {
      return null;
    }

    return this.sortContext.find((s) => s.field === field);
  }

  sort(context: SortContext) {
    if (!this.sorting) {
      return;
    }

    if (!this.sortContext || !this.sortContext?.length) {
      this.sortContext.push(context);
    } else {
      const targetContext = this.findSortContext(context.field);
      targetContext.order = context.order;
    }

    this.tableService.onSort(this.sortContext);
    this.onSort.emit(context);
  }

  onPageChange(page: number) {
    if (!this.pagination) {
      return;
    }

    this.onPage.emit(page);
  }

  trackBy(index, item: TableData<T>) {
    return item?.id;
  }

  onElementsNumberChange(count: number) {
    if (!this.elementNumber) {
      return;
    }

    this.onElementsNumber.emit(count);
  }

  private buildTableItems(items: T[]): TableData<T>[] {
    return items?.map((x) => {
      const storedItemKey = JSON.stringify(x);
      const storedItemIdId = this._storedTableItemsIds[storedItemKey];
      const tableItemId =
        storedItemIdId ?? x[this.bindIdentifier] ?? Guid.newGuid();

      if (!storedItemIdId) {
        this._storedTableItemsIds[storedItemKey] = tableItemId;
      }

      return { id: tableItemId, item: x };
    });
  }
}
