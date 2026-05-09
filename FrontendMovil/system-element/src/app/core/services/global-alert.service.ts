import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { AlertType } from 'src/app/shared/components/alert/models/alert';

export interface Alert {
  type: AlertType;
  title?: string;
  description?: string;
}

@Injectable({ providedIn: 'root' })
export class GlobalAlertService {
  private alert = new Subject<Alert>();
  alert$ = this.alert.asObservable();

  showDanger(title?: string, description?: string) {
    this.show('danger', title, description);
  }

  showSuccess(title?: string, description?: string) {
    this.show('success', title, description);
  }

  showWarning(title?: string, description?: string) {
    this.show('warning', title, description);
  }

  showInfo(title?: string, description?: string) {
    this.show('info', title, description);
  }

  show(type: AlertType, title?: string, description?: string) {
    this.alert.next({ type, title, description });
  }

  hide() {
    this.alert.next(null);
  }
}
