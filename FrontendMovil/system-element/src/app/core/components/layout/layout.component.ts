import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';
import { NavService } from '../sidebar/nav.service';
import { Section } from '../../models/section';
import { GlobalAlertContainerComponent } from '../global-alert-container/global-alert-container.component';
import { ComponentBase } from 'src/app/shared/bases/component-base';
import { takeUntil } from 'rxjs';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    SidebarComponent,
    TopbarComponent,
    GlobalAlertContainerComponent,
  ],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
})
export class LayoutComponent extends ComponentBase implements OnInit {
  public sidebarHideable = false;

  constructor(private navService: NavService, private route: ActivatedRoute) {
    super();
  }

  ngOnInit(): void {
    this.route.data
      .pipe(takeUntil(this.unsubscribed$))
      .subscribe(({ sections }) => {
        this.buildSectionNavs(sections);
      });
  }

  onChangeSidebarHideable() {
    this.sidebarHideable = !this.sidebarHideable;
  }

  private buildSectionNavs(sections: Section[]) {
    sections.forEach((x) => {
      this.navService.add({
        id: x.id,
        name: x.name,
        path: x.sectionPath,
      });
    });
  }
}
