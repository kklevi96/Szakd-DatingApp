import { Component, computed, DestroyRef, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { finalize, Subscription, switchMap } from 'rxjs';
import { AuthService, UserMeResponse } from '../../core/services/auth-service';
import { User } from '../../core/models/user.models';
import { CompatibilityService } from '../../core/services/compatibility-service';
import { SignalRService } from '../../core/services/signalr.service';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { HobbyService } from '../../core/services/hobby-service';
import { MatButtonModule } from '@angular/material/button';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ConversationService } from '../../core/services/conversation-service';
import { CreateConversationRequest } from '../../core/models/conversation.models';
import { MatDialog } from '@angular/material/dialog';
import { MeetingDialog } from '../meetings/meeting-dialog/meeting-dialog';
import { Meeting, MeetingDialogData } from '../../core/models/meeting.models';

@Component({
  selector: 'app-friendship',
  imports: [
    MatSlideToggleModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
  ],
  templateUrl: './friendship.html',
  styleUrl: './friendship.scss',
})
export class Friendship implements OnInit {
  user = signal<UserMeResponse | null>(null);
  wantsFriendship = signal(false);
  isSaving = signal(false);
  errorMessage = signal('');
  users = signal<User[]>([]);
  isLoadingUsers = signal(false);
  hasLoadedUsers = signal(false);
  myHobbyIds = signal(new Set<number>());
  selectedUserIds = signal<string[]>([]);
  openingUserId = signal<string | null>(null);
  isStartingGroup = signal(false);
  conversationError = signal('');
  isOpeningConversation = computed(() =>
    this.openingUserId() !== null || this.isStartingGroup(),
  );

  private usersRequest?: Subscription;

  constructor(
    private auth: AuthService,
    private destroyRef: DestroyRef,
    private compatibility: CompatibilityService,
    private signalRService: SignalRService,
    private hobbies: HobbyService,
    private conversationService: ConversationService,
    private router: Router,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.signalRService.potentialFriendsChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.user() && !this.isSaving() && this.auth.getToken()) {
          this.getUsers();
        }
      });
    this.auth
      .getMe()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.wantsFriendship.set(user.wantsFriendship);
          this.getUsers();
        },
        error: () => {
          this.errorMessage.set('Your profile could not be loaded.');
        },
      });
  }

  setFriendshipEnabled(enabled: boolean): void {
    const user = this.user();

    if (!user || this.isSaving() || this.isOpeningConversation()) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');
    this.wantsFriendship.set(enabled);

    this.auth
      .updateSearchPreferences({
        wantsRelationship: user.wantsRelationship,
        wantsFriendship: enabled,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSaving.set(false)),
      )
      .subscribe({
        next: () => {
          this.user.set({ ...user, wantsFriendship: enabled });
          this.getUsers();
        },
        error: () => {
          this.wantsFriendship.set(user.wantsFriendship);
          this.errorMessage.set('Your friendship preference could not be saved.');
        },
      });
  }

  toggleUserSelection(userId: string): void {
    if (this.isSaving() || this.isOpeningConversation() || !this.wantsFriendship() ||
        !this.users().some(person => person.id === userId)) {
      return;
    }

    this.selectedUserIds.update(ids =>
      ids.includes(userId) ? ids.filter(id => id !== userId) : [...ids, userId],
    );
  }

  openConversation(otherUserId: string): void {
    this.createConversation([otherUserId], false);
  }

  inviteToMeeting(otherUserIds: string[]): void {
    if (this.isSaving() || this.isOpeningConversation() || !this.wantsFriendship() || otherUserIds.length === 0) { return; }
    const token = this.auth.getToken();
    if (!token) { return; }
    const people = this.users().filter(person => otherUserIds.includes(person.id));
    if (people.length !== otherUserIds.length) { return; }
    this.dialog.open<MeetingDialog, MeetingDialogData, Meeting>(MeetingDialog, {
      width: '480px', maxWidth: 'calc(100vw - 24px)',
      data: { otherUserIds: [...otherUserIds], recipientNames: people.map(person => person.firstName) },
    }).afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(meeting => {
      if (meeting && this.auth.getToken() === token) {
        void this.router.navigate(['/conversations'], {
          queryParams: { conversationId: meeting.conversationId, invitations: true },
        });
      }
    });
  }

  startGroupConversation(): void {
    const selectedUserIds = this.selectedUserIds();
    if (selectedUserIds.length < 2) {
      return;
    }

    this.createConversation(selectedUserIds, true);
  }

  private createConversation(otherUserIds: string[], isGroup: boolean): void {
    if (this.isSaving() || this.isOpeningConversation() || !this.wantsFriendship()) {
      return;
    }

    const token = this.auth.getToken();
    if (!token) { return; }

    const request = new CreateConversationRequest();
    request.isGroup = isGroup;
    request.otherUserIds = [...otherUserIds];
    this.openingUserId.set(isGroup ? null : otherUserIds[0]);
    this.isStartingGroup.set(isGroup);
    this.conversationError.set('');

    this.conversationService.createConversation(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.openingUserId.set(null);
          this.isStartingGroup.set(false);
        }),
      )
      .subscribe({
        next: conversation => {
          if (this.auth.getToken() !== token) { return; }

          void this.router.navigate(['/conversations'], {
            queryParams: { conversationId: conversation.id },
          });
        },
        error: (error: HttpErrorResponse) => {
          if (this.auth.getToken() !== token) { return; }

          this.conversationError.set(
            typeof error.error === 'string' && error.error.trim()
              ? error.error
              : 'The conversation could not be opened. Please try again.',
          );
        },
      });
  }

  getUsers(): void {
    this.usersRequest?.unsubscribe();
    this.errorMessage.set('');

    if (!this.wantsFriendship()) {
      this.users.set([]);
      this.selectedUserIds.set([]);
      this.conversationError.set('');
      this.hasLoadedUsers.set(false);
      return;
    }

    this.isLoadingUsers.set(true);

    let myHobbyIds = new Set<number>();

    this.usersRequest = this.hobbies
      .getMy()
      .pipe(
        switchMap(hobbies => {
          myHobbyIds = new Set(hobbies.map(hobby => hobby.id));
          return this.compatibility.getFriendshipUsers();
        }),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoadingUsers.set(false)),
      )
      .subscribe({
        next: (users) => {
          this.myHobbyIds.set(myHobbyIds);
          this.users.set(
            users.map(person => ({
              ...person,
              hobbies: [...person.hobbies].sort(
                (a, b) =>
                  Number(myHobbyIds.has(b.id)) - Number(myHobbyIds.has(a.id)) ||
                  a.name.localeCompare(b.name, 'hu'),
              ),
            })),
          );
          const availableUserIds = new Set(users.map(person => person.id));
          this.selectedUserIds.update(ids => ids.filter(id => availableUserIds.has(id)));
          this.hasLoadedUsers.set(true);
        },
        error: () => {
          this.errorMessage.set('Users could not be loaded.');
        },
      });
  }
}
