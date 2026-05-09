import { HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const AuthorizationInterceptor = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const router = inject(Router);
  return next(req).pipe(
    catchError((err) => {
      if (err.status === 401 && !router.url.includes('/entrar')) {
        router.navigate(['/entrar'], { queryParams: { notAuthorized: 1 } });
      }

      return throwError(() => err);
    })
  );
};
