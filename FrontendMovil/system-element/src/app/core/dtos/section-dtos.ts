export interface ServerResultOutput<T> {
  estado: string;
  mensaje: string;
  modoDeAcceso: string;
  total: number;
  consola: number;
  datos: T;
}

export interface SectionDefinitionDto {
  id: string;
  controlador: string;
  negocio: string;
  modoDeAcceso: string;
  nombre: string;
  parametros?: SectionDefinitionParameters[];
}

export interface SectionDefinitionParameters {
  parametro: string;
  valor: string;
}

export interface RegisterList<T> {
  registros: T;
}

export interface SectionDefinitionListOutput
  extends ServerResultOutput<SectionDefinitionDto[]> {}

export interface ControllerSearchInput {
  filtro?: ControllerSearchFilter[];
}

export type FilterCriteria =
  | 'igual'
  | 'mayor'
  | 'menor'
  | 'esNulo'
  | 'noEsNulo'
  | 'contiene'
  | 'noContiene'
  | 'comienza'
  | 'termina'
  | 'mayorIgual'
  | 'menorIgual'
  | 'diferente'
  | 'esAlgunoDe'
  | 'entreFechas'
  | 'porReferencia'
  | 'deRelacion'
  | 'deTipos'
  | 'entreImportes'
  | 'noEstaRelacionado'
  | 'entreRangos';

export interface ControllerSearchFilter {
  clausula: string;
  criterio: FilterCriteria;
  valor: string;
}

export interface ControllerListOutput
  extends ServerResultOutput<RegisterList<ControllerDto[]>> {}

export interface DomainListOutput extends ServerResultOutput<ControllerDto[]> {}

export interface CabinetDto {
  consola: string;
  datos: ControllerDto;
  estado: string;
  mensaje: string;
  modoDeAcceso: string;
  total: number;
}

export interface ControllerDto {
  baja: boolean;
  cantidad: number;
  carpetas: any;
  cg: string;
  conCarpetas: boolean;
  creadoEl: Date;
  creador: string;
  descripción: string;
  expresion: string;
  id: number;
  idCg: number;
  idCreador: number;
  idModificador: number;
  idTipo: number;
  modificadoEl: Date;
  modificador: string;
  modoDeAcceso: string;
  nombre: string;
  referencia: string;
  sincronizar: any;
  sincronizarCon: any;
  tipo: string;
}

export interface FolderHierarchyDto {
  activo: boolean;
  id: number;
  idPadre?: number;
  negocio?: string;
  nombre?: string;
  tipoDtm?: string;
  tipoDto?: string;
}

export interface FolderHierarchyBranchDto {
  dto?: FolderHierarchyDto;
  hijos?: FolderHierarchyBranchDto[];
}

export interface HierarchyBranchListOutput {
  ramas: FolderHierarchyBranchDto[];
}

export interface FolderHierarchyBranchListOutput
  extends ServerResultOutput<HierarchyBranchListOutput> {}

export interface FileDto {
  id: string;
  nombre: string;
  modoDeAcceso: string;
  creadoEl: Date;
  creador: string;
}

export interface FileListOutput extends ServerResultOutput<FileDto[]> {}

export interface FileUploadOutput extends ServerResultOutput<FileDto> {}
