export interface NavBase {
  id: string;
  icon?: string[];
  name: string;
  extraClasses?: string[];
  path?: string;
  visible?: boolean;
}

export interface Nav extends NavBase {}

export const navItems: Nav[] = [];
