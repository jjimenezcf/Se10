import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, of } from 'rxjs';
import {
  CabinetDto,
  FileListOutput,
  FileUploadOutput,
  FolderHierarchyBranchListOutput,
  SectionDefinitionListOutput,
} from '../dtos/section-dtos';

@Injectable({ providedIn: 'root' })
export class SectionServerService {
  constructor(private http: HttpClient) {}

  getSections(): Observable<SectionDefinitionListOutput> {
    return this.http.get<SectionDefinitionListOutput>('/movil/sections');
  }

  searchByController<T>(controller: string, input: any): Observable<T> {
    return this.http.post<T>('/movil/' + controller + '/search', input);
  }

  searchCabinetsByDomain<T>(domain: string, id: number): Observable<T> {
    return this.http.get<T>('/movil/archivadores/' + domain + '/' + id);
  }

  getFolderHierarchyByCabinetId(
    id: number
  ): Observable<FolderHierarchyBranchListOutput> {
    return this.http.get<FolderHierarchyBranchListOutput>(
      '/movil/carpetas/archivador/' + id
    );
  }

  getCabinetByControllerAndId(
    controller: string,
    id: number
  ): Observable<CabinetDto> {
    return this.http.get<CabinetDto>('/movil/' + controller + '/' + id);
  }

  getFilesByDomainAndId(
    domain: string,
    id: number
  ): Observable<FileListOutput> {
    return this.http.get<FileListOutput>(
      '/movil/archivos/anexos/' + domain + '/' + id
    );
  }

  downloadFile(id: number) {
    return this.http
      .get('/movil/archivos/' + id + '/download', {
        observe: 'response',
        responseType: 'blob',
      })
      .pipe(
        map((result) => {
          let filename = 'undefined';
          const contentDisposition = result.headers.get('content-disposition');
          const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
          const matches = filenameRegex.exec(contentDisposition);
          if (matches != null && matches[1]) {
            filename = matches[1].replace(/['"]/g, '');
          }

          return {
            blob: result.body,
            documentName: filename,
          };
        })
      );
  }

  uploadFile(
    domain: string,
    id: number,
    file: File
  ): Observable<FileUploadOutput> {
    const content = new FormData();
    if (file !== null && file !== undefined) {
      content.append('file', file, file.name);
    }

    return this.http.post<FileUploadOutput>(
      '/movil/archivos/anexos/' + domain + '/' + id + '/upload',
      content
    );
  }
}
