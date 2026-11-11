import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CreateMeetingRequest, Meeting } from '../models/meeting.models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MeetingService {
  constructor(private http: HttpClient) {}

  getMeetings(conversationId?: number) {
    const params: { conversationId?: number } = {};
    if (conversationId !== undefined) {
      params.conversationId = conversationId;
    }
    return this.http.get<Meeting[]>(environment.apiUrl + '/meetings', { params });
  }

  createMeeting(request: CreateMeetingRequest) {
    return this.http.post<Meeting>(environment.apiUrl + '/meetings', request);
  }

  joinMeeting(meetingId: number) {
    return this.http.put<void>(environment.apiUrl + '/meetings/' + meetingId + '/join', {});
  }

  leaveMeeting(meetingId: number) {
    return this.http.delete<void>(environment.apiUrl + '/meetings/' + meetingId + '/leave');
  }
}
