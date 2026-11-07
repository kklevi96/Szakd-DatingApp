import { Component, DestroyRef, EventEmitter, OnInit, Output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatChipsModule, MatChipSelectionChange } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { finalize, forkJoin } from 'rxjs';
import { Hobby as HobbyModel } from '../../core/models/hobby.models';
import { HobbyService } from '../../core/services/hobby-service';

@Component({
  selector: 'app-hobby',
  imports: [ MatChipsModule, MatButtonModule, MatSnackBarModule],
  templateUrl: './hobby.html',
  styleUrl: './hobby.scss',
})
export class Hobby implements OnInit {
  hobbies = signal<HobbyModel[]>([]);
  selectedIds = signal(new Set<number>());
  pendingIds = signal(new Set<number>());
  isLoading = signal(true);
  errorMessage = signal('');

  @Output() savingChange = new EventEmitter<boolean>();

  constructor(
    private hobbyService: HobbyService,
    private snackBar: MatSnackBar,
    private destroyRef: DestroyRef,
  ) {}

  ngOnInit(): void {
    this.loadHobbies();
  }

  loadHobbies(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    forkJoin({ all: this.hobbyService.getAll(), my: this.hobbyService.getMy() }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isLoading.set(false)),
    ).subscribe({
      next: ({ all, my }) => {
        this.hobbies.set(all);
        this.selectedIds.set(new Set(my.map(hobby => hobby.id)));
      },
      error: () => this.errorMessage.set('Hobbies could not be loaded. Please try again.'),
    });
  }

  changeHobby(hobby: HobbyModel, event: MatChipSelectionChange): void {
    if (!event.isUserInput || this.pendingIds().has(hobby.id)) {
      return;
    }
    const wasSelected = this.selectedIds().has(hobby.id);
    this.setSelected(hobby.id, event.selected);
    this.pendingIds.update(ids => new Set(ids).add(hobby.id));
    this.savingChange.emit(true);
    const request = event.selected
      ? this.hobbyService.add(hobby.id)
      : this.hobbyService.remove(hobby.id);
    request.pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => {
        this.pendingIds.update(ids => {
          const remaining = new Set(ids);
          remaining.delete(hobby.id);
          return remaining;
        });
        this.savingChange.emit(this.pendingIds().size > 0);
      }),
    ).subscribe({
      error: () => {
        this.setSelected(hobby.id, wasSelected);
        this.snackBar.open('Your hobby could not be saved. Please try again.', 'Close', {
          duration: 5000,
        });
      },
    });
  }

  private setSelected(id: number, selected: boolean): void {
    this.selectedIds.update(ids => {
      const updated = new Set(ids);
      selected ? updated.add(id) : updated.delete(id);
      return updated;
    });
  }
}
