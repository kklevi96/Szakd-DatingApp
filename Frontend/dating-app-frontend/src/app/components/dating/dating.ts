import { Component, DestroyRef, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { auditTime, finalize, Subscription, switchMap } from 'rxjs';
import { AuthService, UserMeResponse } from '../../core/services/auth-service';
import { HobbyService } from '../../core/services/hobby-service';
import { CompatibilityService } from '../../core/services/compatibility-service';
import { CompatibleUser } from '../../core/models/compatibility.models';
import { Hobby } from '../../core/models/hobby.models';
import { SignalRService } from '../../core/services/signalr.service';

interface DatingCard {
  user: CompatibleUser;
  difference: number;
  hobbies: (Hobby & { isShared: boolean })[];
}

@Component({
  selector: 'app-dating',
  imports: [MatSlideToggleModule, MatCardModule, MatChipsModule, MatButtonModule, MatIconModule],
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
  private matchesRequest?: Subscription;
  private refreshPending = false;

  constructor(
    private auth: AuthService,
    private hobbies: HobbyService,
    private compatibility: CompatibilityService,
    private destroyRef: DestroyRef,
    public signalRService: SignalRService,
  ) {}

  ngOnInit(): void {
    this.signalRService.matchesChanged$.pipe(
      auditTime(250),
      takeUntilDestroyed(this.destroyRef),
    ).subscribe(() => this.refreshFromNotification());

    this.loadProfile();
  }

  loadProfile(keepCards = false): void {
    this.matchesRequest?.unsubscribe();
    if (!keepCards) { this.cards.set([]); }
    this.errorMessage.set('');
    this.isLoadingProfile.set(true);
    const token = this.auth.getToken();
    this.auth.getMe().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.isLoadingProfile.set(false);
        this.runPendingRefresh();
      }),
    ).subscribe({
      next: user => {
        if (this.auth.getToken() !== token) { return; }
        this.user.set(user);
        this.wantsRelationship.set(user.wantsRelationship);
        if (user.wantsRelationship) { this.loadMatches(keepCards); }
        else { this.cards.set([]); }
      },
      error: () => {
        if (this.auth.getToken() === token) {
          this.errorMessage.set('Your profile could not be loaded. Please try again.');
        }
      },
    });
  }

  setDatingEnabled(enabled: boolean): void {
    const user = this.user();
    if (!user || this.isSaving() || this.isLoadingProfile()) { return; }
    const previous = user.wantsRelationship;
    const previousCards = this.cards();
    this.isSaving.set(true);
    this.matchesRequest?.unsubscribe();
    this.wantsRelationship.set(enabled);
    this.cards.set([]);
    this.errorMessage.set('');
    const token = this.auth.getToken();
    this.auth.updateSearchPreferences({
      wantsRelationship: enabled,
      wantsFriendship: user.wantsFriendship,
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.isSaving.set(false);
        this.runPendingRefresh();
      }),
    ).subscribe({
      next: () => {
        if (this.auth.getToken() !== token) { return; }
        this.user.set({ ...user, wantsRelationship: enabled });
        if (enabled) { this.loadMatches(); }
      },
      error: () => {
        if (this.auth.getToken() !== token) { return; }
        this.wantsRelationship.set(previous);
        this.cards.set(previousCards);
        this.errorMessage.set('Your dating preference could not be saved. Please try again.');
      },
    });
  }

  loadMatches(keepCards = false): void {
    const user = this.user();
    this.matchesRequest?.unsubscribe();
    if (!user || !this.wantsRelationship()) { return; }
    if (!keepCards) { this.cards.set([]); }
    this.errorMessage.set('');
    this.isLoadingMatches.set(true);
    let myHobbyIds = new Set<number>();
    const token = this.auth.getToken();
    this.matchesRequest = this.hobbies.getMy().pipe(
      switchMap(hobbies => {
        myHobbyIds = new Set(hobbies.map(hobby => hobby.id));
        return this.compatibility.generateMy();
      }),
      switchMap(() => this.compatibility.getMy()),
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.isLoadingMatches.set(false);
        this.runPendingRefresh();
      }),
    ).subscribe({
      next: matches => {
        if (this.auth.getToken() !== token) { return; }
        const people = matches
          .map(match => match.userId_A === user.id ? match.userB : match.userA)
          .filter((person): person is CompatibleUser => person != null && person.id !== user.id);
        this.cards.set(people.map(person => ({
          user: person,
          difference: Math.abs(person.mbtiCode - user.mbtiCode),
          hobbies: person.hobbies.map(hobby => ({ ...hobby, isShared: myHobbyIds.has(hobby.id) }))
            .sort((a, b) => Number(b.isShared) - Number(a.isShared) || a.name.localeCompare(b.name, 'hu')),
        }))
          .filter(card => card.difference <= 3)
          .sort((a, b) => a.difference - b.difference || a.user.firstName.localeCompare(b.user.firstName, 'hu')));
      },
      error: () => {
        if (this.auth.getToken() === token) {
          this.errorMessage.set('Matches could not be loaded. Please try again.');
        }
      },
    });
  }

  private refreshFromNotification(): void {
    if (this.destroyRef.destroyed || !this.auth.getToken()) { return; }

    if (this.isLoadingProfile() || this.isLoadingMatches() || this.isSaving()) {
      this.refreshPending = true;
      return;
    }

    this.loadProfile(true);
  }

  private runPendingRefresh(): void {
    if (!this.refreshPending || this.isLoadingProfile() || this.isLoadingMatches() || this.isSaving()) {
      return;
    }

    this.refreshPending = false;
    this.refreshFromNotification();
  }
}
