import { Component, DestroyRef, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { finalize, Subscription, switchMap } from 'rxjs';
import { AuthService, UserMeResponse } from '../../core/services/auth-service';
import { HobbyService } from '../../core/services/hobby-service';
import { CompatibilityService } from '../../core/services/compatibility-service';
import { CompatibleUser } from '../../core/models/compatibility.models';
import { Hobby } from '../../core/models/hobby.models';
import { SignalRService } from '../../core/services/signalr.service';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ConversationService } from '../../core/services/conversation-service';
import { CreateConversationRequest } from '../../core/models/conversation.models';

interface DatingCard {
  user: CompatibleUser;
  difference: number;
  hobbies: (Hobby & { isShared: boolean })[];
}

@Component({
  selector: 'app-dating',
  imports: [
    MatSlideToggleModule,
    MatCardModule,
    MatChipsModule,
    MatButtonModule,
    MatIconModule,
  ],
  templateUrl: './dating.html',
  styleUrl: './dating.scss',
})
export class Dating implements OnInit {
  user = signal<UserMeResponse | null>(null);
  wantsRelationship = signal(false);
  cards = signal<DatingCard[]>([]);
  isLoadingProfile = signal(true);
  isLoadingMatches = signal(false);
  isSaving = signal(false);
  errorMessage = signal('');
  openingUserId = signal<string | null>(null);

  private profileRequest?: Subscription;
  private matchesRequest?: Subscription;

  constructor(
    private auth: AuthService,
    private hobbies: HobbyService,
    private compatibility: CompatibilityService,
    private destroyRef: DestroyRef,
    public signalRService: SignalRService,
    private conversationService: ConversationService,
    private router: Router,
  ) { }

  ngOnInit(): void {
    this.signalRService.matchesChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (!this.isSaving() && this.auth.getToken()) {
          this.loadProfile(true);
        }
      });

    this.loadProfile();
  }

  loadProfile(keepCards = false): void {
    this.profileRequest?.unsubscribe();
    this.matchesRequest?.unsubscribe();

    if (!keepCards) {
      this.cards.set([]);
    }

    this.errorMessage.set('');
    this.isLoadingProfile.set(!this.user());

    const token = this.auth.getToken();

    this.profileRequest = this.auth
      .getMe()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoadingProfile.set(false)),
      )
      .subscribe({
        next: user => {
          if (this.auth.getToken() !== token) {
            return;
          }

          this.user.set(user);
          this.wantsRelationship.set(user.wantsRelationship);

          if (user.wantsRelationship) {
            this.loadMatches(keepCards);
          } else {
            this.cards.set([]);
          }
        },
        error: () => {
          if (this.auth.getToken() === token) {
            this.errorMessage.set(
              'Your profile could not be loaded. Please try again.',
            );
          }
        },
      });
  }

  setDatingEnabled(enabled: boolean): void {
    const user = this.user();

    if (!user || this.isSaving() || this.isLoadingProfile()) {
      return;
    }

    const previousCards = this.cards();
    const token = this.auth.getToken();

    this.profileRequest?.unsubscribe();
    this.matchesRequest?.unsubscribe();
    this.isSaving.set(true);
    this.wantsRelationship.set(enabled);
    this.cards.set([]);
    this.errorMessage.set('');

    this.auth
      .updateSearchPreferences({
        wantsRelationship: enabled,
        wantsFriendship: user.wantsFriendship,
      })
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isSaving.set(false)),
      )
      .subscribe({
        next: () => {
          if (this.auth.getToken() === token) {
            this.loadProfile(true);
          }
        },
        error: () => {
          if (this.auth.getToken() !== token) {
            return;
          }

          this.wantsRelationship.set(user.wantsRelationship);
          this.cards.set(previousCards);
          this.loadProfile(true);
          this.errorMessage.set(
            'Your dating preference could not be saved. Please try again.',
          );
        },
      });
  }

  openConversation(otherUserId: string): void {
    if (this.openingUserId() !== null || this.isSaving()) {
      return;
    }

    const token = this.auth.getToken();
    if (!token) { return; }

    const request = new CreateConversationRequest();
    request.otherUserIds = [otherUserId];
    this.openingUserId.set(otherUserId);
    this.errorMessage.set('');

    this.conversationService.createConversation(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.openingUserId.set(null)),
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

          this.errorMessage.set(
            typeof error.error === 'string' && error.error.trim()
              ? error.error
              : 'The conversation could not be opened. Please try again.',
          );
        },
      });
  }

  loadMatches(keepCards = false): void {
    const user = this.user();

    this.matchesRequest?.unsubscribe();

    if (!user || !this.wantsRelationship()) {
      return;
    }

    if (!keepCards) {
      this.cards.set([]);
    }

    this.isLoadingMatches.set(true);

    let myHobbyIds = new Set<number>();
    const token = this.auth.getToken();

    this.matchesRequest = this.hobbies
      .getMy()
      .pipe(
        switchMap(hobbies => {
          myHobbyIds = new Set(hobbies.map(hobby => hobby.id));
          return this.compatibility.generateMy();
        }),
        switchMap(() => this.compatibility.getMy()),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.isLoadingMatches.set(false)),
      )
      .subscribe({
        next: matches => {
          if (this.auth.getToken() !== token) {
            return;
          }

          const people = matches
            .map(match =>
              match.userId_A === user.id ? match.userB : match.userA,
            )
            .filter(
              (person): person is CompatibleUser =>
                person != null && person.id !== user.id,
            );

          this.cards.set(
            people
              .map(person => ({
                user: person,
                difference: Math.abs(person.mbtiCode - user.mbtiCode),
                hobbies: person.hobbies
                  .map(hobby => ({
                    ...hobby,
                    isShared: myHobbyIds.has(hobby.id),
                  }))
                  .sort(
                    (a, b) =>
                      Number(b.isShared) - Number(a.isShared) ||
                      a.name.localeCompare(b.name, 'hu'),
                  ),
              }))
              .filter(card => card.difference <= 3)
              .sort(
                (a, b) =>
                  a.difference - b.difference ||
                  a.user.firstName.localeCompare(b.user.firstName, 'hu'),
              ),
          );
        },
        error: () => {
          if (this.auth.getToken() === token) {
            this.errorMessage.set(
              'Matches could not be loaded. Please try again.',
            );
          }
        },
      });
  }
}
