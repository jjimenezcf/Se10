import {
  ChangeDetectionStrategy,
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  TemplateRef,
} from '@angular/core';
import { AlertType, AlertSetting } from '../models/alert';
import { AlertConfig } from '../models/alert-config';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule, FontAwesomeModule],
  template: `<div
    class="border-t-4 rounded px-4 py-3"
    [class]="alertConf.class"
    role="alert"
    [ngClass]="classes"
    [ngStyle]="styles"
    *ngIf="type"
  >
    <div class="flex items-center">
      <ng-container
        [ngTemplateOutlet]="content ?? defaultContent"
      ></ng-container>
      <ng-template #defaultContent>
        <div class="py-1 mr-2" *ngIf="icon && alertConf?.icon">
          <fa-icon
            [fixedWidth]="true"
            [icon]="alertConf.icon"
            size="2x"
          ></fa-icon>
        </div>
        <div>
          <p class="font-bold">
            <ng-content select="[title]"></ng-content>
            <span *ngIf="title">{{ title }}</span>
          </p>
          <p class="text-sm">
            <ng-content select="[desc]"></ng-content>
            <span *ngIf="description">{{ description }}</span>
          </p>
        </div>
      </ng-template>
    </div>
  </div> `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AlertComponent implements OnChanges {
  @Input({ required: true }) type: AlertType;
  @Input() icon: boolean = true;
  @Input() title: string | null = null;
  @Input() description: string | null = null;
  @Input() content: TemplateRef<any> | null = null;
  @Input() classes: string[] | string | null = null;
  @Input() styles: {
    [klass: string]: any;
  } = {};

  private _alertConf: AlertSetting | null = null;

  constructor(private defaultConf: AlertConfig) {}

  get alertConf(): AlertSetting | null {
    return this._alertConf;
  }

  set alertConf(setting: AlertSetting | null) {
    this._alertConf = setting;
  }

  private loadConfiguration() {
    this.icon = this.icon ?? this.defaultConf.icon;
    this.alertConf = this.defaultConf?.settings[this.type];
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['type'] && this.type) {
      this.loadConfiguration();
    }
  }
}
