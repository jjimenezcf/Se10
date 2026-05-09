import {
  ChangeDetectorRef,
  Directive,
  EventEmitter,
  Input,
  Output,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { Page, PaginationContext } from '../models/pager';
import { PaginationService } from '../services/pagination.service';

@Directive({ selector: '[appPagination]', exportAs: 'pagination' })
export class PaginationDirective {
  @Input() id: string;
  @Input() maxSize: number = 7;
  @Output() pageChange: EventEmitter<number> = new EventEmitter<number>();
  @Output() pageBoundsCorrection: EventEmitter<number> =
    new EventEmitter<number>();

  pages: Page[] = [];

  private changeSub: Subscription;

  constructor(
    private paginationService: PaginationService,
    private changeDetectorRef: ChangeDetectorRef
  ) {
    this.changeSub = this.paginationService.change.subscribe((id) => {
      if (this.id === id) {
        this.updatePageLinks();
        this.changeDetectorRef.markForCheck();
        this.changeDetectorRef.detectChanges();
      }
    });
  }

  ngOnInit() {
    if (this.id === undefined) {
      this.id = this.paginationService.defaultId;
    }

    this.updatePageLinks();
  }

  ngOnChanges(changes: any) {
    this.updatePageLinks();
  }

  ngOnDestroy() {
    this.changeSub.unsubscribe();
  }

  previous() {
    this.checkValidId();
    this.setCurrent(this.getCurrent() - 1);
  }

  next() {
    this.checkValidId();
    this.setCurrent(this.getCurrent() + 1);
  }

  isFirstPage(): boolean {
    return this.getCurrent() === 1;
  }

  isLastPage(): boolean {
    return this.getLastPage() === this.getCurrent();
  }

  setPage(): boolean {
    return this.getLastPage() === this.getCurrent();
  }

  setCurrent(page: number) {
    this.pageChange.emit(page);
  }

  getCurrent(): number {
    return this.paginationService.getCurrentPage(this.id);
  }

  getLastPage(): number {
    let inst = this.paginationService.getContext(this.id);
    if (inst.totalItems < 1) {
      return 1;
    }
    return Math.ceil(inst.totalItems / inst.itemsPerPage);
  }

  getTotalItems(): number {
    return this.paginationService.getContext(this.id).totalItems;
  }

  private checkValidId() {
    if (this.paginationService.getContext(this.id).id == null) {
      console.warn(
        `PaginationDirective: the specified id "${this.id}" does not match any registered PaginationContext`
      );
    }
  }

  private updatePageLinks() {
    let inst = this.paginationService.getContext(this.id);
    const correctedCurrentPage = this.outOfBoundCorrection(inst);

    if (correctedCurrentPage !== inst.currentPage) {
      setTimeout(() => {
        this.pageBoundsCorrection.emit(correctedCurrentPage);
        this.pages = this.createPageArray(
          inst.currentPage,
          inst.itemsPerPage,
          inst.totalItems,
          this.maxSize
        );
      });
    } else {
      this.pages = this.createPageArray(
        inst.currentPage,
        inst.itemsPerPage,
        inst.totalItems,
        this.maxSize
      );
    }
  }

  private outOfBoundCorrection(instance: PaginationContext): number {
    const totalPages = Math.ceil(instance.totalItems / instance.itemsPerPage);
    if (totalPages < instance.currentPage && 0 < totalPages) {
      return totalPages;
    } else if (instance.currentPage < 1) {
      return 1;
    }

    return instance.currentPage;
  }

  private createPageArray(
    currentPage: number,
    itemsPerPage: number,
    totalItems: number,
    paginationRange: number
  ): Page[] {
    paginationRange = +paginationRange;
    let pages = [];

    const totalPages = Math.max(Math.ceil(totalItems / itemsPerPage), 1);
    const halfWay = Math.ceil(paginationRange / 2);

    const isStart = currentPage <= halfWay;
    const isEnd = totalPages - halfWay < currentPage;
    const isMiddle = !isStart && !isEnd;

    let ellipsesNeeded = paginationRange < totalPages;
    let i = 1;

    while (i <= totalPages && i <= paginationRange) {
      let label;
      let pageNumber = this.calculatePageNumber(
        i,
        currentPage,
        paginationRange,
        totalPages
      );
      let openingEllipsesNeeded = i === 2 && (isMiddle || isEnd);
      let closingEllipsesNeeded =
        i === paginationRange - 1 && (isMiddle || isStart);
      if (ellipsesNeeded && (openingEllipsesNeeded || closingEllipsesNeeded)) {
        label = '...';
      } else {
        label = pageNumber;
      }
      pages.push({
        label: label,
        value: pageNumber,
      });
      i++;
    }
    return pages;
  }

  private calculatePageNumber(
    i: number,
    currentPage: number,
    paginationRange: number,
    totalPages: number
  ) {
    let halfWay = Math.ceil(paginationRange / 2);
    if (i === paginationRange) {
      return totalPages;
    } else if (i === 1) {
      return i;
    } else if (paginationRange < totalPages) {
      if (totalPages - halfWay < currentPage) {
        return totalPages - paginationRange + i;
      } else if (halfWay < currentPage) {
        return currentPage - halfWay + i;
      } else {
        return i;
      }
    } else {
      return i;
    }
  }
}
