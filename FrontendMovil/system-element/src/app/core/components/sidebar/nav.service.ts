import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Nav, navItems } from './nav';

@Injectable({ providedIn: 'root' })
export class NavService {
  private items = new BehaviorSubject<Nav[] | null>(this.itemsCopy);
  public items$ = this.items.asObservable();

  constructor() {}

  add(nav: Nav) {
    let navItems = this.items.value;
    if (navItems.some((x) => x.id === nav.id && x.name === nav.name)) {
      navItems = navItems.filter((x) => x.id !== nav.id);
    }
    navItems.push(nav);
    this.items.next(navItems);
  }

  remove(id: string) {
    let navItems = this.items.value;
    navItems = navItems.filter((x) => x.id !== id);
    this.items.next(navItems);
  }

  private get itemsCopy(): Nav[] {
    return navItems.map((x) => {
      return { ...x };
    });
  }
}
