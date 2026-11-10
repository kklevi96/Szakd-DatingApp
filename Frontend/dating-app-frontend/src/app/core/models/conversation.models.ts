import { User } from './user.models';

export class Conversation {
  id = 0;
  createdAt = '';
  isGroup = false;
  name: string | null = null;
  memberUserIds: string[] = [];
  members: User[] = [];
}

export class CreateConversationRequest {
  isGroup = false;
  otherUserIds: string[] = [];
  name: string | null = null;
}

export interface ConversationNameChange {
  id: number;
  name: string;
}
