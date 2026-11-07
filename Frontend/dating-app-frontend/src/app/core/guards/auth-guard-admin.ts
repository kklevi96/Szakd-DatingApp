import { Injectable } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, catchError, map, of } from 'rxjs';
import { AuthService } from '../services/auth-service';

@Injectable({ providedIn: 'root' })
export class AuthGuardAdmin implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate(): Observable<boolean | UrlTree> | UrlTree {
    if (!this.authService.getToken()) {
      return this.router.createUrlTree(['/login']);
    }

    // Load roles from the backend so a direct URL or page refresh also works.
    return this.authService.getMe().pipe(
      map(user => user.roles.includes('Admin')
        ? true
        : this.router.createUrlTree(['/home'])),
      catchError(() => of(this.router.createUrlTree([
        this.authService.getToken() ? '/home' : '/login'
      ])))
    );
  }
}
