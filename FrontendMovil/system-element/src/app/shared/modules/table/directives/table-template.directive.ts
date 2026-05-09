import { Directive, Input, TemplateRef } from '@angular/core';
import { TableTemplateType } from '../models/table';

@Directive({ selector: '[appTableTemplate]' })
export class TableTemplateDirective {
  @Input('appTableTemplate') type: TableTemplateType;

  constructor(public template: TemplateRef<any>) {}

  getType() {
    return this.type;
  }
}
