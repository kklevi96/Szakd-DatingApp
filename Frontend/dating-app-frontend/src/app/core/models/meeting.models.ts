import { User } from './user.models';

export class Meeting {
  id = 0;
  locationId = 0;
  conversationId = 0;
  locationName = '';
  locationAddress = '';
  meetingDate = '';
  createdAt = '';
  userIds: string[] = [];
  users: User[] = [];
  invitedUserIds: string[] = [];
  invitedUsers: User[] = [];
}

export class CreateMeetingRequest {
  locationId = 0;
  meetingDate = '';
  conversationId: number | null = null;
  otherUserIds: string[] = [];
}

export interface MeetingDialogData {
  conversationId?: number;
  otherUserIds?: string[];
  recipientNames?: string[];
}
