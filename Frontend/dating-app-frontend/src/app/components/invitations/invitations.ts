import { Component, DestroyRef, Input, OnChanges, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { finalize, Subscription } from 'rxjs';
import { Meeting, MeetingDialogData } from '../../core/models/meeting.models';
import { MeetingService } from '../../core/services/meeting-service';
import { AuthService } from '../../core/services/auth-service';
import { SignalRService } from '../../core/services/signalr.service';
import { MeetingDialog } from '../meetings/meeting-dialog/meeting-dialog';
import { MeetingCard } from '../meetings/meeting-card/meeting-card';

@Component({
  selector: 'app-invitations',
  imports: [MatButtonModule, MatIconModule, MeetingCard],
  templateUrl: './invitations.html',
  styleUrl: './invitations.scss',
})

export class Invitations implements OnInit, OnChanges {
  @Input({ required: true }) conversationId = 0;

  meetings = signal<Meeting[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  private meetingsRequest?: Subscription;

  constructor(
    private meetingService: MeetingService,
    public auth: AuthService,
    private signalRService: SignalRService,
    private dialog: MatDialog,
    private destroyRef: DestroyRef,
  ) {}

  ngOnInit(): void {
    this.signalRService.meetingsChanged$.pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(conversationId => {
        if (this.auth.getToken() && (conversationId === null || conversationId === this.conversationId)) {
          this.loadMeetings();
        }
      });
  }

  ngOnChanges(): void {
    this.meetings.set([]);
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.meetingsRequest?.unsubscribe();
    if (!this.conversationId) { return; }
    const conversationId = this.conversationId;
    const token = this.auth.getToken();
    if (!token) { return; }
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.meetingsRequest = this.meetingService.getMeetings(conversationId).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isLoading.set(false)),
    ).subscribe({
      next: meetings => {
        if (this.conversationId === conversationId && this.auth.getToken() === token) {
          this.meetings.set(meetings);
        }
      },
      error: () => {
        if (this.conversationId === conversationId && this.auth.getToken() === token) {
          this.errorMessage.set('Meetings could not be loaded.');
        }
      },
    });
  }

  createMeeting(): void {
    const conversationId = this.conversationId;
    const token = this.auth.getToken();
    if (!token || !conversationId) { return; }
    this.dialog.open<MeetingDialog, MeetingDialogData, Meeting>(MeetingDialog, {
      width: '480px', maxWidth: 'calc(100vw - 24px)', data: { conversationId },
    }).afterClosed().pipe(takeUntilDestroyed(this.destroyRef)).subscribe(meeting => {
      if (meeting && this.conversationId === conversationId && this.auth.getToken() === token) {
        this.loadMeetings();
      }
    });
  }
}
