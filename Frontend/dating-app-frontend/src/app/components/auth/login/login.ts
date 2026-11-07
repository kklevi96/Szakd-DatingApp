import { Component, signal } from '@angular/core';
import { AuthService } from '../../../core/services/auth-service';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { finalize, switchMap } from 'rxjs';

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    FormsModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  constructor(
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private router: Router
  ) {}

  email = '';
  password = '';
  isLoading = signal(false);

  public sendLoginCredentials(form: NgForm): void {
    if (form.invalid || this.isLoading()) {
      return;
    }

    this.isLoading.set(true);

    this.authService
      .login({
        email: this.email,
        password: this.password,
      })
      .pipe(
        switchMap(response => {
          this.authService.saveToken(response.token);
          return this.authService.getMe();
        }),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: () => {

          this.snackBar.open('Sikeres bejelentkezés!', 'Bezárás', {
            duration: 3000,
          });
          void this.router.navigateByUrl('/home');
        },
        error: () => {
          this.authService.logout();
          this.snackBar.open('Login failed', 'Close', {
            duration: 5000,
          });
        },
      });
  }
}
