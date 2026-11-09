import { User } from './user.models';

export class Message {
  id = 0;
  conversationId = 0;
  senderUserId = '';
  content = '';
  createdAt = '';
  sender: User | null = null;
}

export class SendMessageRequest {
  conversationId = 0;
  content = '';
}
