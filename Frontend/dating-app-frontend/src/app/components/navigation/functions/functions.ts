import { Component, DestroyRef, effect, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatBadgeModule } from '@angular/material/badge';
import { merge, Subscription } from 'rxjs';
import { AuthService } from '../../../core/services/auth-service';
import { ConversationService } from '../../../core/services/conversation-service';
import { SignalRService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-functions',
  imports: [RouterLink, RouterLinkActive, MatIconModule, MatBadgeModule],
  templateUrl: './functions.html',
  styleUrl: './functions.scss',
})
export class Functions implements OnInit {
  unreadConversationCount = signal(0);
  private unreadRequest?: Subscription;

  constructor(
    private auth: AuthService,
    private conversationService: ConversationService,
    private signalRService: SignalRService,
    private destroyRef: DestroyRef,
  ) {
    effect(() => {
      this.auth.currentToken();
      this.unreadConversationCount.set(0);
      this.getUnreadCount();
    });
  }

  ngOnInit(): void {
    merge(
      this.signalRService.messagesChanged$,
      this.signalRService.conversationsChanged$,
      this.conversationService.readStateChanged$,
    ).pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.getUnreadCount());
  }

  private getUnreadCount(): void {
    this.unreadRequest?.unsubscribe();
    const token = this.auth.getToken();
    if (!token) { return; }

    this.unreadRequest = this.conversationService.getUnreadCount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: count => {
          if (this.auth.getToken() === token) {
            this.unreadConversationCount.set(count);
          }
        },
        error: () => {},
      });
  }
}
