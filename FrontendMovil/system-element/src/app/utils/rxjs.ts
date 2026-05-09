import { Observable, defer, tap } from 'rxjs';
import { GlobalAlertService } from '../core/services/global-alert.service';

export function handleStatusError<T>(alertService: GlobalAlertService) {
  return (source: Observable<T>) =>
    defer(() => {
      return source.pipe(
        tap((result) => {
          const status = result['estado'];
          if (status && status === 'Error') {
            const mensaje = result['mensaje'];
            alertService.showDanger(
              mensaje ?? '¡Ha ocurrido un error inesperado!'
            );

            throw new Error(mensaje);
          }

          alertService.hide();
        })
      );
    });
}
