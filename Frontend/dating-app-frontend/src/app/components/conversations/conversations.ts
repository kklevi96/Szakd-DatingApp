import { Component, DestroyRef, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Conversation } from '../../core/models/conversation.models';
import { ConversationService } from '../../core/services/conversation-service';
import { Messages } from './messages/messages';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../core/services/auth-service';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Invitations } from '../invitations/invitations';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-conversations',
  imports: [
    MatListModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatSidenavModule,
    Messages,
    Invitations,
  ],
  templateUrl: './conversations.html',
  styleUrl: './conversations.scss',
})
export class Conversations implements OnInit {
  conversations = signal<Conversation[]>([]);
  isLoading = signal(true);
  errorMessage = signal('');
  selectedConversation = signal<Conversation | null>(null);
  showInvitations = signal(false);
  isMobile = signal(false);

  private swipeStart?: { pointerId: number; x: number; y: number; time: number };

  constructor(
    private conversationService: ConversationService,
    private destroyRef: DestroyRef,
    private auth: AuthService,
    private breakpointObserver: BreakpointObserver,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.breakpointObserver.observe('(max-width: 720px)')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.isMobile.set(result.matches);
        this.cancelSwipe();
      });

    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.selectConversationFromUrl());

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
          this.selectConversationFromUrl();
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
    this.showInvitations.set(false);
    this.selectedConversation.set(conversation);
    void this.router.navigate(['/conversations'], {
      queryParams: { conversationId: conversation.id },
    });
  }

  backToConversations(): void {
    this.showInvitations.set(false);
    this.selectedConversation.set(null);
    this.cancelSwipe();
    void this.router.navigate(['/conversations']);
  }

  private selectConversationFromUrl(): void {
    const id = Number(this.route.snapshot.queryParamMap.get('conversationId'));
    const conversation = this.conversations().find(item => item.id === id);
    this.showInvitations.set(false);
    this.selectedConversation.set(conversation ?? null);
  }

  onPointerDown(event: PointerEvent): void {
    this.cancelSwipe();

    if (!this.isMobile() || event.pointerType !== 'touch' || !event.isPrimary) {
      return;
    }

    if (event.target instanceof Element &&
      event.target.closest('input, textarea, button, a, [contenteditable]')) {
      return;
    }

    this.swipeStart = {
      pointerId: event.pointerId,
      x: event.clientX,
      y: event.clientY,
      time: event.timeStamp,
    };
  }

  onPointerUp(event: PointerEvent): void {
    const start = this.swipeStart;
    this.cancelSwipe();

    if (!start || start.pointerId !== event.pointerId || !this.isMobile()) {
      return;
    }

    const distanceX = event.clientX - start.x;
    const distanceY = event.clientY - start.y;

    if (Math.abs(distanceX) < 60 ||
      Math.abs(distanceX) < Math.abs(distanceY) * 1.5 ||
      event.timeStamp - start.time > 700) {
      return;
    }

    if (distanceX < 0) {
      this.showInvitations.set(true);
    } else if (this.showInvitations()) {
      this.showInvitations.set(false);
    } else {
      this.backToConversations();
    }
  }

  cancelSwipe(): void {
    this.swipeStart = undefined;
  }
}
