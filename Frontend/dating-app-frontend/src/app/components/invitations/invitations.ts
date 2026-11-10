import { Component, DestroyRef, Input, OnChanges, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { finalize, Subscription } from 'rxjs';
import { Meeting, MeetingDialogData } from '../../core/models/meeting.models';
import { MeetingService } from '../../core/services/meeting-service';
import { AuthService } from '../../core/services/auth-service';
import { SignalRService } from '../../core/services/signalr.service';
import { MeetingDialog } from '../meetings/meeting-dialog/meeting-dialog';

@Component({
  selector: 'app-invitations',
  imports: [DatePipe, MatButtonModule, MatCardModule, MatChipsModule, MatIconModule],
  templateUrl: './invitations.html',
  styleUrl: './invitations.scss',
})
export class Invitations implements OnInit, OnChanges {
  @Input({ required: true }) conversationId = 0;

  meetings = signal<Meeting[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  actionError = signal('');
  pendingIds = signal(new Set<number>());
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
    this.actionError.set('');
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
    this.meetingsRequest = this.meetingService.getByConversation(conversationId).pipe(
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
          this.errorMessage.set('Invitations could not be loaded.');
        }
      },
    });
  }

  isInvited(meeting: Meeting): boolean {
    const id = this.auth.currentUser()?.id;
    return !!id && meeting.invitedUserIds.includes(id);
  }

  isAttending(meeting: Meeting): boolean {
    const id = this.auth.currentUser()?.id;
    return !!id && meeting.userIds.includes(id);
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

  respond(meeting: Meeting, action: 'accept' | 'decline' | 'leave' | 'join'): void {
    if (this.pendingIds().has(meeting.id)) { return; }
    if (action === 'leave' && !this.isAttending(meeting)) { return; }
    if ((action === 'accept' || action === 'decline') && !this.isInvited(meeting)) { return; }
    if (action === 'join' && (this.isAttending(meeting) || this.isInvited(meeting))) { return; }
    const token = this.auth.getToken();
    const conversationId = this.conversationId;
    if (!token) { return; }
    this.actionError.set('');
    this.pendingIds.update(ids => new Set(ids).add(meeting.id));
    const request = action === 'accept' || action === 'join' ? this.meetingService.acceptInvitation(meeting.id) :
      action === 'decline' ? this.meetingService.declineInvitation(meeting.id) : this.meetingService.leaveMeeting(meeting.id);
    request.pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.pendingIds.update(ids => { const next = new Set(ids); next.delete(meeting.id); return next; })),
    ).subscribe({
      next: () => {
        if (this.conversationId === conversationId && this.auth.getToken() === token) { this.loadMeetings(); }
      },
      error: () => {
        if (this.conversationId === conversationId && this.auth.getToken() === token) {
          this.actionError.set('Your response could not be saved. Please try again.');
          this.loadMeetings();
        }
      },
    });
  }
}
