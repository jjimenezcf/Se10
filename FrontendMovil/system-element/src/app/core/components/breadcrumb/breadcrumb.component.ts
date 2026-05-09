import {
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  OnInit,
} from '@angular/core';
import { Breadcrumb, BreadcrumbService } from './breadcrumb.service';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { faChevronRight } from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-breadcrumb',
  templateUrl: './breadcrumb.component.html',
  styleUrls: ['./breadcrumb.component.scss'],
  standalone: true,
  imports: [FontAwesomeModule, CommonModule, RouterModule],
})
export class BreadcrumbComponent implements OnInit, AfterViewInit {
  crumbs: Breadcrumb[] = [];
  protected chevronRightIcon = faChevronRight;

  constructor(
    private breadcrumbService: BreadcrumbService,
    private cd: ChangeDetectorRef
  ) {}

  isEllipsisActive(e) {
    // return Utils.isEllipsisActive(e);
  }

  ngAfterViewInit(): void {
    this.cd.detectChanges();
  }

  ngOnInit(): void {
    this.breadcrumbService.breadcrumbs$
      // .pipe(takeUntil(this.unsubscribed$))
      .subscribe((breadcrumbItems) => {
        if (!breadcrumbItems) {
          return;
        }

        this.crumbs = breadcrumbItems;
        this.cd.detectChanges();
      });
  }
}
