import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { LoginInput, LoginOutput } from '../../pages/login/dtos/login-dtos';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AccountServerService {
  constructor(private http: HttpClient) {}

  login(input: LoginInput): Observable<LoginOutput> {
    return this.http.post<LoginOutput>('/movil/login', input);
  }

  logout() {
    return this.http.get('/movil/logout');
  }
}
