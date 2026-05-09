import {
  Directive,
  HostListener,
  Input,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { TableComponent } from '../components/table/table.component';
import { Sort } from '../models/sort';

@Directive({ selector: '[appTableSortableColumn]' })
export class TableSortableColumnDirective<T> implements OnDestroy, OnInit {
  @Input('appTableSortableColumn') field: string;
  @Input() sortableColumnDisabled: boolean;

  sortOrder: Sort;
  sorted: boolean;

  private subscription = new Subscription();

  constructor(public t: TableComponent<T>) {
    if (this.isEnabled()) {
      this.subscription = this.t.tableService.sort$.subscribe((sortContext) => {
        this.updateSortState();
      });
    }
  }

  @HostListener('click', ['$event'])
  onClick(event: MouseEvent) {
    if (this.isEnabled()) {
      this.updateSortState();
      this.t.sort({
        order: this.t.tableService.nextSort(this.sortOrder),
        field: this.field,
      });
    }
  }

  ngOnInit(): void {
    if (this.isEnabled()) {
      this.updateSortState();
    }
  }

  isEnabled(): boolean {
    return this.sortableColumnDisabled !== true;
  }

  updateSortState() {
    this.sorted = this.t.isSorted(this.field);
    this.sortOrder = this.sorted
      ? this.t.findSortContext(this.field)?.order
      : Sort.None;
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
