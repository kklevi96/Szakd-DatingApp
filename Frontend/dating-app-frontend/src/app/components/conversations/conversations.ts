import { Component, DestroyRef, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Conversation } from '../../core/models/conversation.models';
import { ConversationService } from '../../core/services/conversation-service';
import { Messages } from './messages/messages';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth-service';

@Component({
  selector: 'app-conversations',
  imports: [
    MatListModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    Messages,
  ],
  templateUrl: './conversations.html',
  styleUrl: './conversations.scss',
})
export class Conversations implements OnInit {
  conversations = signal<Conversation[]>([]);
  isLoading = signal(true);
  errorMessage = signal('');
  selectedConversation = signal<Conversation | null>(null);

  constructor(
    private conversationService: ConversationService,
    private destroyRef: DestroyRef,
    private auth: AuthService,
  ) {}

  ngOnInit(): void {
    this.getConversations();
  }

  getConversations(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.conversationService
      .getMyConversations()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          this.conversations.set(response);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set(
            'Conversation loading error. Please try again.',
          );
          this.isLoading.set(false);
        },
      });
  }

  getConversationName(conversation: Conversation): string {
    if (conversation.isGroup) {
      return conversation.name || 'Group conversation';
    }

    const currentUserId = this.auth.currentUser()?.id;

    if (!currentUserId) {
      return 'Direct conversation';
    }

    const otherMember = conversation.members.find(
      member => member.id !== currentUserId,
    );

    return otherMember?.firstName || 'Direct conversation';
  }

  selectConversation(conversation: Conversation): void {
    this.selectedConversation.set(conversation);
  }
}
