import { Component, computed, DestroyRef, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { finalize, Subscription } from 'rxjs';
import { Meeting } from '../../core/models/meeting.models';
import { MeetingService } from '../../core/services/meeting-service';
import { AuthService } from '../../core/services/auth-service';
import { SignalRService } from '../../core/services/signalr.service';
import { MeetingCard } from './meeting-card/meeting-card';

@Component({
  selector: 'app-meetings',
  imports: [MatButtonModule, MatProgressSpinnerModule, MatTabsModule, MeetingCard],
  templateUrl: './meetings.html',
  styleUrl: './meetings.scss',
})
export class Meetings implements OnInit {
  meetings = signal<Meeting[]>([]);
  private now = signal(Date.now());
  upcomingMeetings = computed(() => this.meetings()
    .filter(meeting => !this.isPast(meeting) && this.isAttending(meeting)));
  upcomingInvitations = computed(() => this.meetings()
    .filter(meeting => !this.isPast(meeting) && !this.isAttending(meeting)));
  pastMeetings = computed(() => this.meetings()
    .filter(meeting => this.isPast(meeting))
    .sort((a, b) => new Date(b.meetingDate).getTime() - new Date(a.meetingDate).getTime()));
  isLoading = signal(true);
  hasLoaded = signal(false);
  errorMessage = signal('');
  private meetingsRequest?: Subscription;

  constructor(
    private meetingService: MeetingService,
    private auth: AuthService,
    private signalRService: SignalRService,
    private destroyRef: DestroyRef,
  ) {}

  ngOnInit(): void {
    this.signalRService.meetingsChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.loadMeetings());
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.meetingsRequest?.unsubscribe();
    const token = this.auth.getToken();
    if (!token) { return; }

    this.now.set(Date.now());
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.meetingsRequest = this.meetingService.getMeetings().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isLoading.set(false)),
    ).subscribe({
      next: meetings => {
        if (this.auth.getToken() !== token) { return; }
        this.meetings.set(meetings);
        this.hasLoaded.set(true);
      },
      error: () => {
        if (this.auth.getToken() === token) {
          this.errorMessage.set('Meetings could not be loaded. Please try again.');
        }
      },
    });
  }

  private isPast(meeting: Meeting): boolean {
    return new Date(meeting.meetingDate).getTime() < this.now();
  }

  private isAttending(meeting: Meeting): boolean {
    const userId = this.auth.currentUser()?.id;
    return !!userId && meeting.userIds.includes(userId);
  }
}
