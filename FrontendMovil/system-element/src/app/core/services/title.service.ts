import { Injectable } from '@angular/core';
import { Router, ActivatedRoute, NavigationEnd } from '@angular/router';
import { merge, Observable } from 'rxjs';
import { filter, map, distinctUntilChanged, mergeMap } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';
import { BreadcrumbService } from '../components/breadcrumb/breadcrumb.service';

const SEPARATOR = ' - ';

@Injectable({ providedIn: 'root' })
export class TitleService {
  constructor(
    private title: Title,
    private router: Router,
    private route: ActivatedRoute,
    private breadcrumbService: BreadcrumbService
  ) {}

  init(showTitle: boolean = true) {
    merge(this.routeTitles, this.breadcrumbTitles)
      .pipe(filter((result) => !!result))
      .subscribe((result) => {
        this.title.setTitle(
          result + (showTitle ? SEPARATOR + 'Sistema de elementos' : '')
        );
      });
  }

  private get breadcrumbTitles(): Observable<string> {
    return this.breadcrumbService.breadcrumbs$.pipe(
      filter((bc) => !!bc),
      map((bc) => {
        return bc.reduce((acc, frag) => {
          if (acc && frag?.label) {
            acc += SEPARATOR;
          }

          return acc + !frag.loading ? frag.label : 'Loading';
        }, '');
      })
    );
  }

  private get routeTitles(): Observable<string> {
    return this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      distinctUntilChanged(),
      map(() => {
        let route = this.route;
        while (route.firstChild) {
          route = route.firstChild;
        }
        return route;
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
