import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FileComponent } from './file.component';

describe('FileComponent', () => {
  let component: FileComponent;
  let fixture: ComponentFixture<FileComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [FileComponent]
    });
    fixture = TestBed.createComponent(FileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
