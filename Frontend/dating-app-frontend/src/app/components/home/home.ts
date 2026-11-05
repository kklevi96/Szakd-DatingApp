import { Component, inject, OnInit, signal } from '@angular/core';
import { AuthService, UserMeResponse } from '../../core/services/auth-service';

@Component({
  selector: 'app-home',
  imports: [],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit {
  private authService = inject(AuthService);

  user = signal<UserMeResponse | null>(null);
  errorMessage = signal('');

  ngOnInit(): void {
    this.authService.getMe().subscribe({
      next: (user) => {
        this.user.set(user);
      },
      error: () => {
        this.errorMessage.set('Error while requesting user data');
      }
    });
  }
}
