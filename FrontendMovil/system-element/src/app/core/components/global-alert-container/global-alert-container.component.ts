import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AlertComponent } from 'src/app/shared/components/alert/components/alert.component';
import { Alert, GlobalAlertService } from '../../services/global-alert.service';

@Component({
  selector: 'app-global-alert-container',
  standalone: true,
  imports: [CommonModule, AlertComponent],
  templateUrl: './global-alert-container.component.html',
  styleUrls: ['./global-alert-container.component.scss'],
})
export class GlobalAlertContainerComponent {
  alert: Alert;

  constructor(private alertService: GlobalAlertService) {
    this.alertService.alert$.subscribe((alert) => {
      this.alert = alert;
    });
  }
}
