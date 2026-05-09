import { Nav } from './nav';

export class SidebarNav implements Nav {
  id: string;
  icon?: string[];
  name: string;
  extraClasses?: string[];
  path?: string;
  visible?: boolean;
  isCollapsed: boolean;

  constructor(item: Nav) {
    this.id = item.id;
    this.icon = item.icon;
    // this.fontawesomeIcon = item.icon;
    this.name = item.name;
    this.extraClasses = item?.extraClasses;
    this.path = item?.path;
    this.visible = item?.visible;
    this.isCollapsed = true;
  }

}
