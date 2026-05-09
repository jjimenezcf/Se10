import { Injectable } from '@angular/core';
import { BehaviorSubject, filter } from 'rxjs';
import { Section } from '../models/section';

@Injectable({ providedIn: 'root' })
export class SectionService {
  private sections = new BehaviorSubject<Section[]>([]);
  $sections = this.sections.asObservable().pipe(filter((x) => !!x.length));

  setSections(sections: Section[]) {
    this.sections.next(sections);
  }
}
