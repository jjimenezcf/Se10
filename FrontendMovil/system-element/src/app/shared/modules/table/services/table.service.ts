import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { Sort, SortContext } from '../models/sort';

@Injectable()
export class TableService {
  private sort = new Subject<SortContext[]>();
  sort$ = this.sort.asObservable();

  onSort(context: SortContext[]) {
    this.sort.next(context);
  }

  nextSort(sort: Sort) {
    switch (sort) {
      case Sort.Descendant:
        return Sort.None;
      case Sort.Ascendant:
        return Sort.Descendant;
      case Sort.None:
        return Sort.Ascendant;
      default:
        return Sort.None;
    }
  }
}
