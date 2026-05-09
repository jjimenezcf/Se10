import { Injectable } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { filter, Observable, distinctUntilChanged, map, mergeMap } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ScreenTitleService {
  constructor(private router: Router, private route: ActivatedRoute) {}

  get title(): Observable<string> {
    return this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      distinctUntilChanged(),
      map(() => {
        let route = this.route;
        let lastRouteWithTitle: ActivatedRoute = null;
        while (route.firstChild) {
          route = route.firstChild;
          if (route.snapshot.data['title']) {
            lastRouteWithTitle = route;
          }
        }
        return lastRouteWithTitle;
      }),
      filter((route) => route.outlet === 'primary'),
      mergeMap((route) => route.data),
      filter((data) => data['title']),
      map((data) => {
        return data['title'];
      })
    );
  }
}
