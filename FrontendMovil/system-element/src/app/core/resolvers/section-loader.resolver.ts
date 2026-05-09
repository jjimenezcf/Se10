import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { map } from 'rxjs';
import { SectionService } from '../services/section.service';
import { SectionServerService } from '../services/section-server.service';
import { Section } from '../models/section';

export const sectionLoaderResolver = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const sectionServerService = inject(SectionServerService);
  const sectionService = inject(SectionService);

  return sectionServerService.getSections().pipe(
    map((result) => {
      const sections = result.datos.map((x) => {
        const sectionParameters = x.parametros.map((x) => x.valor);

        return {
          accessMode: x.modoDeAcceso,
          domain: x.negocio,
          controller: x.controlador,
          id: x.id,
          name: x.nombre,
          parameters: x.parametros,
          sectionPath: (sectionParameters.length
            ? [x.controlador, ...sectionParameters].join('')
            : x.controlador
          ).toLowerCase(),
        } as Section;
      });

      sectionService.setSections(sections);
      return sections;
    })
  );
};
