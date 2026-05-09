import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  Input,
  OnDestroy,
  OnInit,
  ViewEncapsulation,
} from '@angular/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';
import { Subscription } from 'rxjs';
import { Sort } from '../../models/sort';
import { TableComponent } from '../table/table.component';

@Component({
  selector: 'app-table-sort-icon',
  template: ` <fa-icon [icon]="getSortIcon(sortOrder)"></fa-icon>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class TableSortIconComponent<T> implements OnInit, OnDestroy {
  @Input() field: string;

  sortOrder: Sort;
  subscription = new Subscription();

  constructor(public t: TableComponent<T>, public cd: ChangeDetectorRef) {
    this.subscription = this.t.tableService.sort$.subscribe((sortMeta) => {
      this.updateSortState();
    });
  }

  ngOnInit(): void {
    this.updateSortState();
  }

  getSortIcon(sorting: string): IconProp {
    const sortingIcons: { [key: string]: IconProp } = {
      [Sort.None]: 'sort',
      [Sort.Ascendant]: 'sort-up',
      [Sort.Descendant]: 'sort-down',
    };

    return ['fas', sortingIcons[sorting]] as IconProp;
  }

  private updateSortState() {
    this.sortOrder = this.t.findSortContext(this.field)?.order;
    this.cd.markForCheck();
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
