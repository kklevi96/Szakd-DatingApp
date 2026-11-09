import { Component, effect, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { Navigation } from './components/navigation/navigation';
import { AuthService } from './core/services/auth-service';
import { SignalRService } from './core/services/signalr.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navigation],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('dating-app-frontend');

  constructor(auth: AuthService, signalRService: SignalRService, router: Router) {
    let previousToken = auth.currentToken();

    effect(onCleanup => {
      const token = auth.currentToken();
      if (token) {
        signalRService.startConnection(token);
      } else {
        signalRService.stopConnection();
      }

      if (previousToken && !token) {
        void router.navigateByUrl('/login');
      }

      previousToken = token;
      onCleanup(() => signalRService.stopConnection());
    });
  }
}
