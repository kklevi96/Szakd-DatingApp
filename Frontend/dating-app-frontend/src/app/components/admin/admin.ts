import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize } from 'rxjs';
import { AuthService } from '../../core/services/auth-service';

@Component({
  selector: 'app-admin',
  imports: [MatButtonModule, MatSnackBarModule],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin implements OnInit {
  message = signal('');
  errorMessage = signal('');
  isRemovingAdmin = signal(false);

  constructor(
    public auth: AuthService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.auth.getAdminStatus().subscribe({
      next: response => this.message.set(response.message),
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(error.status === 403
          ? 'Admin access was denied.'
          : 'The admin page could not be loaded.');
      }
    });
  }

  public removeAdmin(): void {
    if (this.isRemovingAdmin() || !this.auth.isAdmin() || !this.message()) {
      return;
    }

    this.errorMessage.set('');
    this.isRemovingAdmin.set(true);

    this.auth.removeAdmin().pipe(
      finalize(() => this.isRemovingAdmin.set(false))
    ).subscribe({
      next: () => {
        this.auth.logout();
        this.snackBar.open('Admin access removed. Please sign in again.', 'Close', {
          duration: 5000
        });
        void this.router.navigate(['/login']);
      },
      error: (error: HttpErrorResponse) => {
        const message = error.error?.message;
        this.errorMessage.set(typeof message === 'string'
          ? message
          : 'Admin access could not be removed. Please try again.');
      }
    });
  }
}
