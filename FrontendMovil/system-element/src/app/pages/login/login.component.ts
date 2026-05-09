import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AccountServerService } from 'src/app/core/services/account-server.service';
import { AuthService } from 'src/app/core/auth/auth.service';
import { ActivatedRoute, Router } from '@angular/router';
import { AlertComponent } from 'src/app/shared/components/alert/components/alert.component';
import { faCircleNotch } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { catchError, finalize, takeUntil, throwError } from 'rxjs';
import { ComponentBase } from 'src/app/shared/bases/component-base';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    AlertComponent,
    FontAwesomeModule,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent extends ComponentBase implements OnInit {
  loginForm: FormGroup;
  loading: boolean;
  logoUrl = null;
  returnUrl: string = '';
  loginError: boolean;
  errorMessage: string;
  protected spinnerIcon = faCircleNotch;

  constructor(
    private loginServerService: AccountServerService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    super();
    this.loginForm = new FormGroup({
      user: new FormControl('', Validators.required),
      password: new FormControl('', Validators.required),
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(takeUntil(this.unsubscribed$))
      .subscribe((params) => {
        const returnUrl = params.get('returnUrl');
        this.returnUrl = returnUrl ?? '/';

        if (params.get('notAuthorized')) {
          this.errorMessage = 'Acceso no disponible';
          this.router.navigate([], { relativeTo: this.route, queryParams: {} });
        }
      });
  }

  onSubmit() {
    if (this.loginForm.invalid) {
      return;
    }

    this.doLogin();
  }

  private doLogin() {
    this.errorMessage = null;
    this.loading = true;
    const controls = this.loginForm.controls;
    this.loginServerService
      .login({
        username: controls['user'].value,
        password: controls['password'].value,
      })
      .pipe(
        finalize(() => (this.loading = false)),
        catchError((err) => {
          this.errorMessage = 'Usuario y/o contraseña incorrectos';
          throw err;
        })
      )
      .subscribe((result) => {
        this.authService.authUser = {
          lastName: result.surname,
          name: result.name,
          userName: result.userName,
        };

        this.router.navigateByUrl(this.returnUrl);
      });
  }
}
