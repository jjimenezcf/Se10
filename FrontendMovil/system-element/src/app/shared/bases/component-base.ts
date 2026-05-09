import { Component, inject, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { GlobalAlertService } from 'src/app/core/services/global-alert.service';

@Component({
  template: '',
})
export abstract class ComponentBase implements OnDestroy {
  protected unsubscribed$ = new Subject<void>();
  protected globalAlertService = inject(GlobalAlertService);

  protected completeObs() {
    if (this.unsubscribed$) {
      this.unsubscribed$.next();
      this.unsubscribed$.complete();
    }
  }

  ngOnDestroy(): void {
    this.completeObs();
    this.globalAlertService.hide();
  }
}
