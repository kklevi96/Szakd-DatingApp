import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest
} from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth-service';
import { environment } from '../../../environments/environment';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const accessToken = this.authService.getToken();

    const isBackendRequest =
      req.url === environment.apiUrl ||
      req.url.startsWith(environment.apiUrl + '/');

    if (!accessToken || !isBackendRequest) {
      return next.handle(req);
    }

    req = req.clone({
      setHeaders: {
        Authorization: 'Bearer ' + accessToken
      }
    });

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && this.authService.getToken() === accessToken) {
          this.authService.logout();
          void this.router.navigate(['/login']);
        }

        return throwError(() => error);
      })
    );
  }
}
