import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableComponent } from './components/table/table.component';
import { TablePagerComponent } from './components/table-pager/table-pager.component';
import { TableTemplateDirective } from './directives/table-template.directive';
import { PaginationDirective } from './directives/pagination.directive';
import { PaginationPipe } from './pipes/pagination.pipe';
import { FormsModule } from '@angular/forms';
import { TableSortableColumnDirective } from './directives/table-sortable-column.directive';
import { TableSortIconComponent } from './components/table-sortable-icon/table-sort-icon.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@NgModule({
  declarations: [
    TableComponent,
    TableComponent,
    TablePagerComponent,
    TableTemplateDirective,
    PaginationDirective,
    PaginationPipe,
    TableSortableColumnDirective,
    TableSortIconComponent,
  ],
  exports: [
    TableComponent,
    TablePagerComponent,
    TableTemplateDirective,
    PaginationDirective,
    PaginationPipe,
    TableSortableColumnDirective,
    TableSortIconComponent,
  ],
  imports: [CommonModule, FormsModule, FontAwesomeModule],
  providers: [],
})
export class TableModule {}
