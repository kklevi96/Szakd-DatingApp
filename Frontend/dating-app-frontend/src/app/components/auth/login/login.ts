import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth-service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, MatSnackBarModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private authService = inject(AuthService);
  private snackBar = inject(MatSnackBar);

  email = '';
  password = '';
  isLoading = false;

  login(): void {
    this.isLoading = true;

    this.authService.login({
      email: this.email,
      password: this.password
    }).subscribe({
      next: (res: any) => {
        this.authService.saveToken(res.token);
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.snackBar.open('Login failed', 'Close', {
          duration: 5000
        });
      }
    });
  }
}
