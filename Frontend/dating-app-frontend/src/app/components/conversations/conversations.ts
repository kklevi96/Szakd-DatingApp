import { afterNextRender, Component, DestroyRef, ElementRef, Injector, OnInit, signal, ViewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Conversation, ConversationNameChange } from '../../core/models/conversation.models';
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
import { MatInputModule } from '@angular/material/input';
import { finalize, Subscription } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { SignalRService } from '../../core/services/signalr.service';

@Component({
  selector: 'app-conversations',
  imports: [
    MatInputModule,
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

  isEditingName = signal(false);
  isSavingName = signal(false);
  nameError = signal('');
  editedName = '';

  @ViewChild('nameInput') private nameInput?: ElementRef<HTMLInputElement>;
  private conversationsRequest?: Subscription;

  private swipeStart?: { pointerId: number; x: number; y: number; time: number };

  constructor(
    private conversationService: ConversationService,
    private destroyRef: DestroyRef,
    private auth: AuthService,
    private breakpointObserver: BreakpointObserver,
    private route: ActivatedRoute,
    private router: Router,
    private signalRService: SignalRService,
    private injector: Injector,
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

    this.signalRService.conversationsChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.auth.getToken()) {
          this.getConversations(true);
        }
      });

    this.signalRService.conversationRenamed$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(change => {
        if (!this.auth.getToken()) { return; }

        if (change === null || !this.conversations().some(item => item.id === change.id)) {
          this.getConversations(true);
        } else {
          this.updateConversationName(change);
          if (this.conversationsRequest && !this.conversationsRequest.closed) {
            this.getConversations(true);
          }
        }
      });

    this.getConversations();
  }

  getConversations(quiet = false): void {
    this.conversationsRequest?.unsubscribe();
    if (!quiet) {
      this.isLoading.set(true);
      this.errorMessage.set('');
    }
    const token = this.auth.getToken();

    this.conversationsRequest = this.conversationService
      .getMyConversations()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          if (this.auth.getToken() !== token) { return; }
          this.conversations.set(response);
          this.errorMessage.set('');
          this.isLoading.set(false);
          const id = Number(this.route.snapshot.queryParamMap.get('conversationId'));
          const selected = response.find(item => item.id === id) ?? null;
          if (selected?.id !== this.selectedConversation()?.id) {
            this.cancelRenaming();
          }
          this.selectedConversation.set(selected);
        },
        error: () => {
          if (this.auth.getToken() !== token) { return; }
          if (!quiet || this.conversations().length === 0) {
            this.errorMessage.set('Conversation loading error. Please try again.');
          }
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

  startRenaming(): void {
    const conversation = this.selectedConversation();
    if (!conversation?.isGroup || this.isSavingName()) { return; }

    this.editedName = this.getConversationName(conversation);
    this.nameError.set('');
    this.isEditingName.set(true);
    afterNextRender(() => {
      this.nameInput?.nativeElement.focus();
      this.nameInput?.nativeElement.select();
    }, { injector: this.injector });
  }

  cancelRenaming(): void {
    this.isEditingName.set(false);
    this.nameError.set('');
    this.editedName = '';
  }

  saveConversationName(): void {
    const conversation = this.selectedConversation();
    if (!this.isEditingName() || !conversation?.isGroup || this.isSavingName()) { return; }

    const name = this.editedName.trim();
    if (!name || name.length > 100) {
      this.nameError.set('The group name must contain between 1 and 100 characters.');
      return;
    }

    if (name === this.getConversationName(conversation)) {
      this.cancelRenaming();
      return;
    }

    const token = this.auth.getToken();
    if (!token) { return; }
    this.nameError.set('');
    this.isSavingName.set(true);
    this.conversationService.renameConversation(conversation.id, name)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSavingName.set(false)),
      )
      .subscribe({
        next: response => {
          if (this.auth.getToken() !== token) { return; }
          this.updateConversationName({ id: response.id, name: response.name! });
          if (this.selectedConversation()?.id === conversation.id) {
            this.cancelRenaming();
          }
        },
        error: (error: HttpErrorResponse) => {
          if (this.auth.getToken() === token && this.selectedConversation()?.id === conversation.id && this.isEditingName()) {
            this.nameError.set(typeof error.error === 'string'
              ? error.error : 'The group name could not be saved. Please try again.');
          }
        },
      });
  }

  private updateConversationName(change: ConversationNameChange): void {
    this.conversations.update(items => items.map(item =>
      item.id === change.id ? { ...item, name: change.name } : item,
    ));
    this.selectedConversation.update(item =>
      item?.id === change.id ? { ...item, name: change.name } : item,
    );
  }

  selectConversation(conversation: Conversation): void {
    this.cancelRenaming();
    this.showInvitations.set(false);
    this.selectedConversation.set(conversation);
    void this.router.navigate(['/conversations'], {
      queryParams: { conversationId: conversation.id },
    });
  }

  backToConversations(): void {
    this.cancelRenaming();
    this.showInvitations.set(false);
    this.selectedConversation.set(null);
    this.cancelSwipe();
    void this.router.navigate(['/conversations']);
  }

  private selectConversationFromUrl(): void {
    this.cancelRenaming();
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
