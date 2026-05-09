import { SectionDefinitionParameters } from '../dtos/section-dtos';

export interface Section {
  id: string;
  sectionPath?: string;
  controller: string;
  domain: string;
  accessMode: string;
  name: string;
  parameters?: SectionDefinitionParameters[];
}

export interface SectionParameters {
  parameter: string;
  value: string;
}
