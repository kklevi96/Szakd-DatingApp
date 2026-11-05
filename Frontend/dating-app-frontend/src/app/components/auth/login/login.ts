import { Component } from '@angular/core';
import { AuthService } from '../../../core/services/auth-service';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';

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
  isLoading = false;

  public sendLoginCredentials(form: NgForm): void {
    if (form.invalid || this.isLoading) {
      return;
    }

    this.isLoading = true;

    this.authService
      .login({
        email: this.email,
        password: this.password,
      })
      .subscribe({
        next: (res: any) => {
          this.authService.saveToken(res.token);
          this.isLoading = false;

          this.snackBar.open('Sikeres bejelentkezés!', 'Bezárás', {
            duration: 3000,
          });
          this.router.navigateByUrl('/home');
        },
        error: () => {
          this.isLoading = false;
          this.snackBar.open('Login failed', 'Close', {
            duration: 5000,
          });
        },
      });
  }
}
