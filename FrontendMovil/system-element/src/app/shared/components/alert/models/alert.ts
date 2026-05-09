import { TemplateRef } from '@angular/core';
import { IconProp } from '@fortawesome/fontawesome-svg-core';

export type AlertType = 'success' | 'danger' | 'warning' | 'info';

export interface AlertSetting {
  class: string;
  icon: IconProp;
}

export interface AlertSettings {
  icon?: boolean;
  settings?: {
    [type in AlertType]: AlertSetting;
  };
}

export class CreateAlertSettings {
  type: AlertType;
  title?: string;
  description?: string;
  icon?: boolean;
  template?: TemplateRef<any>;
  classes?: string[] | string;
  styles?: {
    [klass: string]: any;
  };
}
