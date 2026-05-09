import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'src/app/shared/modules/table/table.module';
import {
  faAngleRight,
  faCircleNotch,
  faFile,
  faSpinner,
} from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-section-table',
  standalone: true,
  imports: [CommonModule, TableModule, FontAwesomeModule],
  templateUrl: './section-table.component.html',
  styleUrls: ['./section-table.component.scss'],
})
export class SectionTableComponent<T> {
  @Input() items: T[];
  @Input() loading: boolean;
  @Input() allowNavigate = true;
  @Output() navigate = new EventEmitter<T>();
  @Output() viewFiles = new EventEmitter<T>();

  protected spinnerIcon = faCircleNotch;
  protected fileIcon = faFile;
  protected angleRight = faAngleRight;
}
