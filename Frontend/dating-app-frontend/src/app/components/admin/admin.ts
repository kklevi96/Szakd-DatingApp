import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AuthService } from '../../core/services/auth-service';

@Component({
  selector: 'app-admin',
  imports: [],
  templateUrl: './admin.html',
  styleUrl: './admin.scss',
})
export class Admin implements OnInit {
  message = signal('');
  errorMessage = signal('');

  constructor(public auth: AuthService) {}

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
}
