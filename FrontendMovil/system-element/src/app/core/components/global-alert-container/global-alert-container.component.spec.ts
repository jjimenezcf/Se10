import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GlobalAlertContainerComponent } from './global-alert-container.component';

describe('GlobalAlertContainerComponent', () => {
  let component: GlobalAlertContainerComponent;
  let fixture: ComponentFixture<GlobalAlertContainerComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [GlobalAlertContainerComponent]
    });
    fixture = TestBed.createComponent(GlobalAlertContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
