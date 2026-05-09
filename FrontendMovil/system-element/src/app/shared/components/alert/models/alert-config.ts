import { Injectable } from '@angular/core';
import { AlertSettings } from './alert';
import {
  faCircleCheck,
  faCircleExclamation,
  faCircleInfo,
  faWarning,
} from '@fortawesome/free-solid-svg-icons';

@Injectable({ providedIn: 'root' })
export class AlertConfig implements AlertSettings {
  icon = true;
  dimissible = false;
  settings = {
    danger: {
      class: 'bg-red-100 border-red-500 text-red-900',
      icon: faCircleExclamation,
    },
    success: {
      class: 'bg-green-100 border-green-500 text-green-900',
      icon: faCircleCheck,
    },
    warning: {
      class: 'bg-yellow-100 border-yellow-500 text-yellow-900',
      icon: faWarning,
    },
    info: {
      class: 'bg-blue-100 border-blue-500 text-blue-900',
      icon: faCircleInfo,
    },
  };
}
