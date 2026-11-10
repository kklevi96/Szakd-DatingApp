import { Component, DestroyRef, Inject, OnInit, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { CreateMeetingRequest, Meeting, MeetingDialogData } from '../../../core/models/meeting.models';
import { Location } from '../../../core/models/location.models';
import { LocationService } from '../../../core/services/location-service';
import { MeetingService } from '../../../core/services/meeting-service';
import { AuthService } from '../../../core/services/auth-service';

@Component({
  selector: 'app-meeting-dialog',
  imports: [FormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './meeting-dialog.html',
  styleUrl: './meeting-dialog.scss',
})
export class MeetingDialog implements OnInit {
  locations = signal<Location[]>([]);
  isLoading = signal(true);
  isSaving = signal(false);
  errorMessage = signal('');
  locationId: number | null = null;
  meetingDate = '';
  minDate = this.localDateTime(new Date());

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: MeetingDialogData,
    public dialogRef: MatDialogRef<MeetingDialog, Meeting>,
    private locationService: LocationService,
    private meetingService: MeetingService,
    private auth: AuthService,
    private destroyRef: DestroyRef,
  ) {}

  ngOnInit(): void { this.loadLocations(); }

  loadLocations(): void {
    this.isLoading.set(true);
    this.errorMessage.set('');
    this.locationService.getAll().pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: locations => { this.locations.set(locations); this.isLoading.set(false); },
        error: () => { this.errorMessage.set('Locations could not be loaded.'); this.isLoading.set(false); },
      });
  }

  sendInvitation(form: NgForm): void {
    if (this.isSaving() || this.isLoading()) { return; }
    form.form.markAllAsTouched();
    this.errorMessage.set('');
    if (form.invalid || !this.locations().some(location => location.id === this.locationId)) { return; }
    const date = new Date(this.meetingDate);
    if (!Number.isFinite(date.getTime()) || date.getTime() <= Date.now()) {
      this.errorMessage.set('Please choose a future date and time.');
      return;
    }
    const token = this.auth.getToken();
    if (!token) { return; }
    const request = new CreateMeetingRequest();
    request.locationId = this.locationId!;
    request.meetingDate = this.meetingDate;
    request.conversationId = this.data.conversationId ?? null;
    request.otherUserIds = [...(this.data.otherUserIds ?? [])];
    this.isSaving.set(true);
    this.dialogRef.disableClose = true;
    this.meetingService.createMeeting(request).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => { this.isSaving.set(false); this.dialogRef.disableClose = false; }),
    ).subscribe({
      next: meeting => { if (this.auth.getToken() === token) { this.dialogRef.close(meeting); } },
      error: (error: HttpErrorResponse) => {
        if (this.auth.getToken() === token) {
          this.errorMessage.set(typeof error.error === 'string' && error.error.trim()
            ? error.error : 'The invitation could not be sent. Please try again.');
        }
      },
    });
  }

  private localDateTime(date: Date): string {
    const pad = (value: number) => String(value).padStart(2, '0');
    return date.getFullYear() + '-' + pad(date.getMonth() + 1) + '-' + pad(date.getDate()) +
      'T' + pad(date.getHours()) + ':' + pad(date.getMinutes());
  }
}
