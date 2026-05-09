import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  Output,
  Renderer2,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { filter, takeUntil } from 'rxjs';
import { Router, RouterModule } from '@angular/router';
import { NavService } from './nav.service';
import { SidebarNav } from './sidebar-nav';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faChevronLeft, faXmark } from '@fortawesome/free-solid-svg-icons';
import { AuthService, User } from '../../auth/auth.service';
import { AccountServerService } from '../../services/account-server.service';
import { ComponentBase } from 'src/app/shared/bases/component-base';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule, FontAwesomeModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent extends ComponentBase implements OnDestroy {
  @Input() hideable: boolean = false;
  @Output() changeHideable = new EventEmitter<void>();

  public items: SidebarNav[] = [];
  protected chevronLeftIcon = faChevronLeft;
  protected xmarkIcon = faXmark;

  authUser: User | null = null;
  nameInitials: string;

  @ViewChild('sidebar') sidebarUser: ElementRef | undefined;

  private windowEventUnlistener: (() => void) | undefined;

  constructor(
    private navService: NavService,
    private router: Router,
    private renderer: Renderer2,
    private authService: AuthService,
    private accountService: AccountServerService
  ) {
    super();
    this.authUser = this.authService.authUser;
    this.extractNameInitials();
  }

  ngOnInit(): void {
    this.fillSidebar();
  }

  onCloseSession() {
    this.authService.removeAuthUser();
    this.accountService.logout().subscribe((_) => {
      this.router.navigateByUrl('entrar');
    });
  }

  isItemActive(item: SidebarNav) {
    return this.isRouteActive([item.path]);
  }

  onClick(item: SidebarNav) {
    if (!item.path || (item.path && item.isCollapsed)) {
      item.isCollapsed = !item.isCollapsed;
    }

    if (this.hideable) {
      this.changeHideable.emit();
    }
  }

  onChangeHideable() {
    this.changeHideable.emit();
  }

  ngAfterViewInit(): void {
    this.windowEventUnlistener = this.renderer.listen(
      'window',
      'click',
      (evt) => {
        if (
          evt.target &&
          this.hideable &&
          !this.sidebarUser?.nativeElement.contains(evt.target)
        ) {
          this.onChangeHideable();
        }
        return evt;
      }
    );
  }

  override ngOnDestroy(): void {
    if (this.windowEventUnlistener) {
      this.windowEventUnlistener();
    }

    super.ngOnDestroy();
  }

  private fillSidebar() {
    this.navService.items$
      .pipe(
        filter((i) => !!i?.length),
        takeUntil(this.unsubscribed$)
      )
      .subscribe((items) => {
        if (!items) {
          return;
        }

        this.items = items.map((i) => new SidebarNav(i));
      });
  }

  private extractNameInitials() {
    const firstInitial = this.authUser.name?.charAt(0) ?? '';
    const secondInitial = this.authUser.lastName?.charAt(0) ?? '';
    this.nameInitials = (firstInitial + secondInitial).toUpperCase();
  }

  private isRouteActive(routeLink: any[]): boolean {
    const tree = this.router.createUrlTree(routeLink);
    const isActive = this.router.isActive(tree, {
      paths: 'subset',
      queryParams: 'subset',
      fragment: 'ignored',
      matrixParams: 'ignored',
    });
    return isActive;
  }
}
