import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  Input,
  Output,
  ViewEncapsulation,
} from '@angular/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';

function coerceToBoolean(input: string | boolean): boolean {
  return !!input && input !== 'false';
}

@Component({
  selector: 'app-table-pager',
  template: `
    <div
      [ngClass]="containerClass"
      [ngStyle]="containerStyle"
      appPagination
      class="mt-4 mb-2"
      [id]="id"
      #p="pagination"
      [maxSize]="maxSize"
      (pageChange)="pageChange.emit($event)"
      (pageBoundsCorrection)="pageBoundsCorrection.emit($event)"
    >
      <nav class="ss-pager">
        <ul *ngIf="!(autoHide && p.pages.length <= 1)">
          <li class="pagination-previous" [class.disabled]="p.isFirstPage()">
            <a
              tabindex="0"
              *ngIf="1 < p.getCurrent()"
              (keyup.enter)="p.previous()"
              (click)="p.previous()"
            >
              <ng-container [ngTemplateOutlet]="previousText"></ng-container>
            </a>
            <span *ngIf="p.isFirstPage()" aria-disabled="true">
              <ng-container [ngTemplateOutlet]="previousText"></ng-container>
            </span>
            <ng-template #previousText
              ><fa-icon [icon]="previousIcon"></fa-icon
            ></ng-template>
          </li>
          <li
            [class.current]="p.getCurrent() === page.value"
            [class.ellipsis]="page.label === '...'"
            *ngFor="let page of p.pages; trackBy: trackByIndex"
          >
            <a
              tabindex="0"
              (keyup.enter)="p.setCurrent(page.value)"
              (click)="p.setCurrent(page.value)"
              *ngIf="p.getCurrent() !== page.value"
            >
              <ng-container [ngTemplateOutlet]="pageText"></ng-container>
            </a>
            <span *ngIf="p.getCurrent() === page.value">
              <ng-container [ngTemplateOutlet]="pageText"></ng-container>
            </span>
            <ng-template #pageText>
              {{
                page.label === '...' ? page.label : (page.label | number : '')
              }}
            </ng-template>
          </li>
          <li class="pagination-next" [class.disabled]="p.isLastPage()">
            <a
              tabindex="0"
              *ngIf="!p.isLastPage()"
              (kFeyup.enter)="p.next()"
              (click)="p.next()"
            >
              <ng-container [ngTemplateOutlet]="nextTemplate"></ng-container>
            </a>
            <span *ngIf="p.isLastPage()" aria-disabled="true">
              <ng-container [ngTemplateOutlet]="nextTemplate"></ng-container>
            </span>
            <ng-template #nextTemplate
              ><fa-icon [icon]="nextIcon"></fa-icon
            ></ng-template>
          </li>
        </ul>
      </nav>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
})
export class TablePagerComponent {
  @Input() id: string;
  @Input() maxSize: number = 10;
  @Input() containerClass: string;
  @Input() containerStyle: any;
  @Input() previousIcon: IconProp = ['fal', 'angle-left'];
  @Input() nextIcon: IconProp = ['fal', 'angle-right'];

  @Input()
  get autoHide(): boolean {
    return this._autoHide;
  }
  set autoHide(value: boolean) {
    this._autoHide = coerceToBoolean(value);
  }

  @Output() pageChange: EventEmitter<number> = new EventEmitter<number>();
  @Output() pageBoundsCorrection: EventEmitter<number> =
    new EventEmitter<number>();

  private _autoHide: boolean = false;

  trackByIndex(index: number) {
    return index;
  }
}
