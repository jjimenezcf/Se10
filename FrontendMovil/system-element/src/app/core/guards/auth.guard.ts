import { inject } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
} from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const authGuard = (
  next: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
) => {
  const router = inject(Router);
  const authService = inject(AuthService);

  if (!authService.isLoggedIn()) {
    const returnUrl =
      state.url === '/' || state.url === '/main' ? undefined : state.url;

    router.navigate(['/entrar'], {
      queryParams: {
        returnUrl,
      },
    });

    return false;
  }

  return true;
};
