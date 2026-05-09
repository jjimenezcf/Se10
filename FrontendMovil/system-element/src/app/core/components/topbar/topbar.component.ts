import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faBars } from '@fortawesome/free-solid-svg-icons';
import { BreadcrumbComponent } from '../breadcrumb/breadcrumb.component';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, FontAwesomeModule, BreadcrumbComponent],
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent {
  @Output() changeSidebarHideable = new EventEmitter<void>();

  constructor() {}

  protected barsIcon = faBars;
}
