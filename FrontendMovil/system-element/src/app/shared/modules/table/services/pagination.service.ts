import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { PaginationContext } from '../models/pager';

const DEFAULT_ID = 'PAGINATION_ID';

@Injectable({ providedIn: 'root' })
export class PaginationService {
  public contexts: { [id: string]: PaginationContext } = {};

  public change = new Subject<string>();

  public register(context: PaginationContext): boolean {
    if (context.id == null) {
      context.id = this.defaultId;
    }

    if (!this.contexts[context.id]) {
      this.contexts[context.id] = context;
      return true;
    } else {
      return this.updateContext(context);
    }
  }

  private updateContext(context: PaginationContext): boolean {
    let changed = false;

    const id = context.id;
    Object.keys(this.contexts[id]).forEach((k) => {
      if (this.contexts[id][k] !== context[k]) {
        this.contexts[id][k] = context[k];
        changed = true;
      }
    });

    return changed;
  }

  public getCurrentPage(id: string): number | null {
    if (!this.contexts[id]) {
      return null;
    }

    return this.contexts[id].currentPage;
  }

  public setCurrentPage(id: string, page: number) {
    let context = this.contexts[id];
    if (!context) {
      return null;
    }

    let maxPage = Math.ceil(context.totalItems / context.itemsPerPage);
    if (page <= maxPage && 1 <= page) {
      context.currentPage = page;
      this.change.next(id);
    }
  }

  public setTotalItems(id: string, total: number) {
    let context = this.contexts[id];
    if (context && 0 <= total) {
      context.totalItems = total;
      this.change.next(id);
    }
  }

  public setItemsPerPage(id: string, itemsPerPage: number) {
    let context = this.contexts[id];
    if (context) {
      context.itemsPerPage = itemsPerPage;
      this.change.next(id);
    }
  }

  public getContext(id = this.defaultId): PaginationContext | null {
    let context = this.contexts[id];
    if (!context) {
      return null;
    }

    return context;
  }

  public get defaultId() {
    return DEFAULT_ID;
  }
}
