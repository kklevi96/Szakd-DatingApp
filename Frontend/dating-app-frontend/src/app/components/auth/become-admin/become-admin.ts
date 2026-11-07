import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule, NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/services/auth-service';

@Component({
  selector: 'app-become-admin',
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './become-admin.html',
  styleUrl: './become-admin.scss',
})
export class BecomeAdmin implements OnInit {
  password = '';
  loadingProfile = signal(true);
  isLoading = signal(false);
  errorMessage = signal('');

  constructor(
    public auth: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.auth.getMe().pipe(
      finalize(() => this.loadingProfile.set(false))
    ).subscribe({
      error: () => this.errorMessage.set('Your profile could not be loaded. Please refresh to try again.')
    });
  }

  public delegateAdmin(form: NgForm): void {
    if (form.invalid || this.isLoading() || !this.auth.currentUser()?.canRequestAdmin) {
      return;
    }

    this.errorMessage.set('');
    this.isLoading.set(true);

    this.auth.delegateAdmin(this.password).pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: () => {
        this.password = '';
        this.auth.logout();
        this.snackBar.open('You are now an admin. Please sign in again.', 'Close', {
          duration: 5000
        });
        void this.router.navigate(['/login']);
      },
      error: (error: HttpErrorResponse) => {
        const message = error.error?.message;
        this.errorMessage.set(typeof message === 'string'
          ? message
          : 'Admin access could not be granted. Please try again.');
      }
    });
  }
}
