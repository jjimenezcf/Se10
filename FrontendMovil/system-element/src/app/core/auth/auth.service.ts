import { Injectable } from '@angular/core';

export const AUTH_USER_STORAGE = 'AUTH_USER_STORAGE';

export interface User {
  userName: string;
  name: string;
  lastName: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  set authUser(user: User | null) {
    if (!user) {
      return;
    }

    localStorage.setItem(AUTH_USER_STORAGE, JSON.stringify(user));
  }

  get authUser(): User | null {
    const authUser = localStorage.getItem(AUTH_USER_STORAGE);
    if (authUser) {
      return JSON.parse(authUser);
    }

    return null;
  }

  constructor() {}

  isLoggedIn() {
    return this.authUser;
  }

  removeAuthUser() {
    localStorage.removeItem(AUTH_USER_STORAGE);
  }
}
