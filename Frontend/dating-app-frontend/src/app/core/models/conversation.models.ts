import { User } from './user.models';

export class Conversation {
  id = 0;
  createdAt = '';
  isGroup = false;
  name: string | null = null;
  memberUserIds: string[] = [];
  members: User[] = [];
}
