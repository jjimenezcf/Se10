import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SectionServerService } from 'src/app/core/services/section-server.service';
import {
  ControllerDto,
  ControllerListOutput,
  ControllerSearchInput,
} from 'src/app/core/dtos/section-dtos';
import { Section } from 'src/app/core/models/section';
import { SectionService } from 'src/app/core/services/section.service';
import { finalize, takeUntil } from 'rxjs';
import { SearchInputComponent } from 'src/app/shared/components/search-input/search-input.component';
import { SectionTableComponent } from 'src/app/shared/components/section-table/section-table.component';
import { ComponentBase } from 'src/app/shared/bases/component-base';
import { handleStatusError } from 'src/app/utils/rxjs';

@Component({
  selector: 'app-section',
  standalone: true,
  imports: [CommonModule, SectionTableComponent, SearchInputComponent],
  templateUrl: './section.component.html',
  styleUrls: ['./section.component.scss'],
})
export class SectionComponent extends ComponentBase implements OnInit {
  section: Section;
  items: ControllerDto[] = [];
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
      this.initData(sectionPath);
    });
  }

  search(expression?: string) {
    let search = {} as ControllerSearchInput;
    if (this.section.parameters) {
      search.filtro = this.section.parameters.map((x) => {
        return {
          clausula: x.parametro,
          criterio: 'igual',
          valor: x.valor,
        };
      });
    }

    if (expression) {
      search.filtro.push({
        clausula: 'expresion',
        criterio: 'contiene',
        valor: expression,
      });
    }

    this.loading = true;
    this.sectionServerService
      .searchByController<ControllerListOutput>(this.section.controller, search)
      .pipe(
        finalize(() => (this.loading = false)),
        handleStatusError(this.globalAlertService)
      )
      .subscribe((result) => {
        this.items = result.datos.registros;
      });
  }

  navigateToCabinet(item: ControllerDto) {
    this.router.navigateByUrl(
      this.section.sectionPath + '/archivadores/' + item.id
    );
  }

  navigateToFiles(item: ControllerDto) {
    this.router.navigateByUrl(
      this.section.sectionPath +
        '/ficheros/' +
        this.section.domain +
        '/' +
        item.id
    );
  }

  private initData(section: string) {
    this.sectionService.$sections
      .pipe(takeUntil(this.unsubscribed$))
      .subscribe((sections) => {
        this.section = sections.find((x) => x.sectionPath === section);
        this.search();
      });
  }
}
