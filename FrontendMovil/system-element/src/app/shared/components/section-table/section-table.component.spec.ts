import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SectionTableComponent } from './section-table.component';

describe('SectionTableComponent', () => {
  let component: SectionTableComponent<any>;
  let fixture: ComponentFixture<SectionTableComponent<any>>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [SectionTableComponent],
    });
    fixture = TestBed.createComponent(SectionTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
