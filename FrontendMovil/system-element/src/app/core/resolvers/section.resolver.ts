import { inject } from '@angular/core';
import { Router, RouterStateSnapshot } from '@angular/router';
import { ActivatedRouteSnapshot } from '@angular/router';
import { BreadcrumbService } from '../components/breadcrumb/breadcrumb.service';
import { SectionService } from '../services/section.service';
import { map, of } from 'rxjs';

export const sectionResolver = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const breadcrumbService = inject(BreadcrumbService);
  const router = inject(Router);
  const sectionService = inject(SectionService);
  const sectionPath = route.paramMap.get('sectionPath');

  return sectionService.$sections.pipe(
    map((sections) => {
      const section = sections.find((x) => x.sectionPath === sectionPath);
      if (!section) {
        router.navigateByUrl('/');
        return null;
      }

      breadcrumbService.setForAlias('sectionName', section.name);
      return section;
    })
  );
};
