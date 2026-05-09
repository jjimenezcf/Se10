import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { SectionServerService } from 'src/app/core/services/section-server.service';
import { SectionService } from 'src/app/core/services/section.service';
import { Section } from 'src/app/core/models/section';
import {
  FolderHierarchyBranchDto,
  FolderHierarchyBranchListOutput,
} from 'src/app/core/dtos/section-dtos';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faCaretDown,
  faCaretRight,
  faFolder,
  faFolderOpen,
} from '@fortawesome/free-solid-svg-icons';
import { Collapsible } from 'src/app/utils/collapse';
import { ComponentBase } from 'src/app/shared/bases/component-base';
import { takeUntil } from 'rxjs';
import { handleStatusError } from 'src/app/utils/rxjs';

@Component({
  standalone: true,
  imports: [CommonModule, FontAwesomeModule],
  templateUrl: './folder.component.html',
  styleUrls: ['./folder.component.scss'],
})
export class FolderComponent extends ComponentBase implements OnInit {
  cabinetId: number;
  sourceCabinetId: number;
  section: Section;
  hierarchy: FolderHierarchyBranchListOutput;
  folderIcon = faFolder;
  folderOpenIcon = faFolderOpen;
  arrowRightIcon = faCaretRight;
  arrowDownIcon = faCaretDown;
  collapsed = new Collapsible<string>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private sectionServerService: SectionServerService,
    private sectionService: SectionService
  ) {
    super();
  }

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntil(this.unsubscribed$)).subscribe((x) => {
      const sectionPath = x.get('sectionPath');
      this.cabinetId = +x.get('cabinetId');
      this.sourceCabinetId = +x.get('sourceCabinetId');

      this.initData(sectionPath);
    });
  }

  getItemKey(branch: FolderHierarchyBranchDto): string {
    return branch.dto.id + branch.dto.negocio;
  }

  getFolderHierarchy() {
    this.sectionServerService
      .getFolderHierarchyByCabinetId(this.cabinetId)
      .pipe(handleStatusError(this.globalAlertService))
      .subscribe((result) => {
        this.hierarchy = result;
      });
  }

  goToFiles(id: number, domain: string) {
    if (domain === 'Carpetas') {
      domain = 'Carpeta';
    } else if (domain === 'Archivadores') {
      domain = 'Archivador';
    }

    this.router.navigateByUrl(
      this.section.sectionPath +
        '/archivadores/' +
        this.sourceCabinetId +
        '/carpetas/' +
        this.cabinetId +
        '/ficheros/' +
        domain +
        '/' +
        id
    );
  }

  private initData(section: string) {
    this.sectionService.$sections
      .pipe(takeUntil(this.unsubscribed$))
      .subscribe((sections) => {
        this.section = sections.find((x) => x.sectionPath === section);
        this.getFolderHierarchy();
      });
  }
}
