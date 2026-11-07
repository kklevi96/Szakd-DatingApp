import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { Component, DestroyRef, OnDestroy, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { finalize } from 'rxjs';
import { AuthService, UserMeResponse } from '../../core/services/auth-service';
import { Hobby } from '../../core/models/hobby.models';
import { HobbyService } from '../../core/services/hobby-service';
import { HobbyDialog } from '../hobby/hobby-dialog/hobby-dialog';

@Component({
  selector: 'app-home',
  imports: [MatDialogModule, MatButtonModule, MatChipsModule, FormsModule, MatCheckboxModule],
  templateUrl: './home.html',
  styleUrl: './home.scss',
})
export class Home implements OnInit, OnDestroy {
  user = signal<UserMeResponse | null>(null);
  errorMessage = signal('');
  wantsRelationship = false;
  wantsFriendship = false;
  isSavingPreferences = signal(false);
  preferencesMessage = signal('');
  preferencesError = signal('');
  hobbies = signal<Hobby[]>([]);
  hobbiesError = signal('');
  isLoadingHobbies = signal(true);
  private hobbyDialog?: MatDialogRef<HobbyDialog>;

  constructor(
    private authService: AuthService,
    private hobbyService: HobbyService,
    private dialog: MatDialog,
    private destroyRef: DestroyRef,
  ) {}

  ngOnInit(): void {
    this.authService.getMe().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: user => {
        this.user.set(user);
        this.wantsRelationship = user.wantsRelationship;
        this.wantsFriendship = user.wantsFriendship;
        this.loadHobbies();
      },
      error: () => this.errorMessage.set('Error while requesting user data'),
    });
  }

  saveSearchPreferences(): void {
    if (this.isSavingPreferences()) { return; }
    const preferences = { wantsRelationship: this.wantsRelationship, wantsFriendship: this.wantsFriendship };
    this.isSavingPreferences.set(true);
    this.preferencesMessage.set('');
    this.preferencesError.set('');
    this.authService.updateSearchPreferences(preferences).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isSavingPreferences.set(false)),
    ).subscribe({
      next: () => {
        this.user.update(user => user ? { ...user, ...preferences } : null);
        this.preferencesMessage.set('Search preferences saved.');
      },
      error: () => this.preferencesError.set('Search preferences could not be saved. Please try again.'),
    });
  }

  loadHobbies(): void {
    this.isLoadingHobbies.set(true);
    this.hobbiesError.set('');
    this.hobbyService.getMy().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isLoadingHobbies.set(false)),
    ).subscribe({
      next: hobbies => {
        this.hobbies.set(hobbies);
        if (!this.authService.hasCheckedHobbies()) {
          this.authService.markHobbiesChecked();
          if (hobbies.length === 0) {
            this.editHobbies();
          }
        }
      },
      error: () => this.hobbiesError.set('Your hobbies could not be loaded. Please try again.'),
    });
  }

  editHobbies(): void {
    if (this.hobbyDialog) { return; }
    this.hobbyDialog = this.dialog.open(HobbyDialog, {
      width: '560px',
      maxWidth: '95vw',
    });
    this.hobbyDialog.afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.hobbyDialog = undefined;
      this.loadHobbies();
    });
  }

  ngOnDestroy(): void {
    this.hobbyDialog?.close();
  }
}
