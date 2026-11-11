import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Conversation, CreateConversationRequest } from '../models/conversation.models';
import { environment } from '../../../environments/environment';
import { Subject, tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ConversationService {
  http: HttpClient;

  private readStateChanged = new Subject<void>();
  readonly readStateChanged$ = this.readStateChanged.asObservable();

  constructor(http: HttpClient) {
    this.http = http;
  }

  getMyConversations() {
    return this.http.get<Conversation[]>(
      environment.apiUrl + '/conversations/my',
    );
  }

  getUnreadCount() {
    return this.http.get<number>(
      environment.apiUrl + '/conversations/unread-count',
    );
  }

  markAsRead(conversationId: number, lastReadMessageId: number) {
    return this.http.put<void>(
      environment.apiUrl + '/conversations/' + conversationId + '/read',
      { lastReadMessageId },
    ).pipe(tap(() => this.readStateChanged.next()));
  }

  renameConversation(conversationId: number, name: string) {
    return this.http.put<Conversation>(
      environment.apiUrl + '/conversations/' + conversationId + '/name',
      { name },
    );
  }

  createConversation(request: CreateConversationRequest) {
    return this.http.post<Conversation>(
      environment.apiUrl + '/conversations',
      request,
    );
  }
}

