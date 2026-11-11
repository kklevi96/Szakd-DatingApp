import { Component, DestroyRef, EventEmitter, Input, Output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { finalize, Observable } from 'rxjs';
import { Meeting } from '../../../core/models/meeting.models';
import { AuthService } from '../../../core/services/auth-service';
import { MeetingService } from '../../../core/services/meeting-service';

@Component({
  selector: 'app-meeting-card',
  imports: [DatePipe, RouterLink, MatButtonModule, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './meeting-card.html',
  styleUrl: './meeting-card.scss',
})
export class MeetingCard {
  @Input({ required: true }) meeting = new Meeting();
  @Input() showConversationLink = true;
  @Input() showActions = true;
  @Output() changed = new EventEmitter<void>();

  isSaving = signal(false);
  errorMessage = signal('');

  constructor(
    public auth: AuthService,
    private meetingService: MeetingService,
    private destroyRef: DestroyRef,
  ) {}

  isAttending(): boolean {
    const id = this.auth.currentUser()?.id;
    return !!id && this.meeting.userIds.includes(id);
  }

  joinMeeting(): void {
    this.saveAttendance(this.meetingService.joinMeeting(this.meeting.id));
  }

  leaveMeeting(): void {
    this.saveAttendance(this.meetingService.leaveMeeting(this.meeting.id));
  }

  private saveAttendance(request: Observable<void>): void {
    if (this.isSaving() || !this.auth.getToken()) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');
    request.pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isSaving.set(false)),
    ).subscribe({
      next: () => this.changed.emit(),
      error: () => this.errorMessage.set('The meeting could not be updated. Please try again.'),
    });
  }
}
