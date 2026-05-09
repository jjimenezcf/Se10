import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SectionServerService } from 'src/app/core/services/section-server.service';
import { TableModule } from 'src/app/shared/modules/table/table.module';
import { FileDto } from 'src/app/core/dtos/section-dtos';
import { DocumentUtils } from 'src/app/utils/document-utils';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { finalize } from 'rxjs';
import { faCircleNotch } from '@fortawesome/free-solid-svg-icons';
import { handleStatusError } from 'src/app/utils/rxjs';
import { GlobalAlertService } from 'src/app/core/services/global-alert.service';
import { FileName } from 'src/app/utils/file-name';

@Component({
  standalone: true,
  imports: [CommonModule, TableModule, FontAwesomeModule],
  templateUrl: './file.component.html',
  styleUrls: ['./file.component.scss'],
})
export class FileComponent implements OnInit {
  items: FileDto[];
  id: number;
  domain: string;
  file: File;
  loadingFiles: boolean;
  uploadingFiles: boolean;
  downloadinFiles: { [id: string]: boolean } = {};

  protected spinnerIcon = faCircleNotch;

  @ViewChild('fileInput') fileInput: ElementRef;
  @ViewChild('mobileFileInput') mobileFileInput: ElementRef;

  constructor(
    private route: ActivatedRoute,
    private sectionService: SectionServerService,
    private globalAlertService: GlobalAlertService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe((result) => {
      this.domain = result.get('domainName');
      this.id = +result.get('itemId');
      this.getFiles();
    });
  }

  getFiles() {
    this.loadingFiles = true;
    this.isPhone;
    this.sectionService
      .getFilesByDomainAndId(this.domain, this.id)
      .pipe(
        finalize(() => (this.loadingFiles = false)),
        handleStatusError(this.globalAlertService)
      )
      .subscribe((result) => {
        this.items = result.datos;
      });
  }

  downloadFile(id: number) {
    this.downloadinFiles[id] = true;
    this.sectionService
      .downloadFile(id)
      .pipe(
        finalize(() => delete this.downloadinFiles[id]),
        handleStatusError(this.globalAlertService)
      )
      .subscribe((result) => {
        DocumentUtils.downloadFile(result.blob, result.documentName);
      });
  }

  get isPhone(): boolean {
    return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
      navigator.userAgent
    );
  }

  fileChangeListener($event: any): void {
    let file = $event.srcElement?.files[0];
    if (!file) {
      return;
    }

    this.file = file;
    if (this.isPhone && this.mobileFileInput?.nativeElement?.value) {
      this.file = new File(
        [this.file],
        `${FileName.generateNameFromDate(new Date())}.png`,
        {
          type: this.file.type,
        }
      );
    }

    if (this.fileInput.nativeElement.value) {
      this.fileInput.nativeElement.value = null;
    } else if (this.mobileFileInput.nativeElement.value) {
      this.mobileFileInput.nativeElement.value = null;
    }
  }

  removeFile() {
    this.file = null;
  }

  uploadFile() {
    this.uploadingFiles = true;
    this.sectionService
      .uploadFile(this.domain, this.id, this.file)
      .pipe(
        finalize(() => (this.uploadingFiles = false)),
        handleStatusError(this.globalAlertService)
      )
      .subscribe((result) => {
        this.file = null;
        this.getFiles();
      });
  }
}
