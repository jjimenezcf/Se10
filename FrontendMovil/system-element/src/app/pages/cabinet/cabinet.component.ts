import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SectionServerService } from 'src/app/core/services/section-server.service';
import { SectionService } from 'src/app/core/services/section.service';
import { Section } from 'src/app/core/models/section';
import {
  ControllerDto,
  ControllerSearchInput,
  DomainListOutput,
} from 'src/app/core/dtos/section-dtos';
import { finalize, takeUntil } from 'rxjs';
import { SectionTableComponent } from 'src/app/shared/components/section-table/section-table.component';
import { ComponentBase } from 'src/app/shared/bases/component-base';
import { handleStatusError } from 'src/app/utils/rxjs';

@Component({
  standalone: true,
  imports: [CommonModule, SectionTableComponent],
  templateUrl: './cabinet.component.html',
  styleUrls: ['./cabinet.component.scss'],
})
export class CabinetComponent extends ComponentBase implements OnInit {
  id: number;
  section: Section;
  items: ControllerDto[] = [];
  cabinet: ControllerDto;
  loading: boolean;

  constructor(
    private route: ActivatedRoute,
    private sectionServerService: SectionServerService,
    private sectionService: SectionService,
    private router: Router
  ) {
    super();
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.unsubscribed$)).subscribe((x) => {
      const sectionPath = x.get('sectionPath');
      this.id = +x.get('sourceCabinetId');
      this.initData(sectionPath);
    });
  }

  search(expression?: string) {
    let search = {} as ControllerSearchInput;

    if (expression) {
      search.filtro.push({
        clausula: 'expresion',
        criterio: 'contiene',
        valor: expression,
      });
    }

    this.loading = true;
    this.sectionServerService
      .searchCabinetsByDomain<DomainListOutput>(this.section.domain, this.id)
      .pipe(
        finalize(() => (this.loading = false)),
        handleStatusError(this.globalAlertService)
      )
      .subscribe((result) => {
        this.items = result.datos;
      });
  }

  private getCabinetData() {
    this.sectionServerService
      .getCabinetByControllerAndId(this.section.controller, this.id)
      .pipe(handleStatusError(this.globalAlertService))
      .subscribe((result) => {
        this.cabinet = result.datos;
      });
  }

  private initData(section: string) {
    this.sectionService.$sections
      .pipe(takeUntil(this.unsubscribed$))
      .subscribe((sections) => {
        this.section = sections.find((x) => x.sectionPath === section);
        this.search();
        this.getCabinetData();
      });
  }

  navigateToCabinet(item: ControllerDto) {
    this.router.navigateByUrl(
      this.section.sectionPath +
        '/archivadores/' +
        this.id +
        '/carpetas/' +
        item.id
    );
  }

  navigateToFiles(item: ControllerDto) {
    this.router.navigateByUrl(
      this.section.sectionPath +
        '/archivadores/' +
        this.id +
        '/ficheros/archivador/' +
        item.id
    );
  }
}
