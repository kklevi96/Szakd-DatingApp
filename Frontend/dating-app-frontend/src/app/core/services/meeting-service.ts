import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateMeetingRequest, Meeting } from '../models/meeting.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MeetingService {
  constructor(private http: HttpClient) {}

  getByConversation(conversationId: number) {
    return this.http.get<Meeting[]>(environment.apiUrl + '/meetings/conversation/' + conversationId);
  }

  createMeeting(request: CreateMeetingRequest) {
    return this.http.post<Meeting>(environment.apiUrl + '/meetings', request);
  }

  acceptInvitation(meetingId: number) {
    return this.http.put<void>(environment.apiUrl + '/meetings/' + meetingId + '/join', {});
  }

  declineInvitation(meetingId: number) {
    return this.http.delete<void>(environment.apiUrl + '/meetings/' + meetingId + '/invitation');
  }

  leaveMeeting(meetingId: number) {
    return this.http.delete<void>(environment.apiUrl + '/meetings/' + meetingId + '/leave');
  }
}
