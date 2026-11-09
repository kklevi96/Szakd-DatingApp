import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Conversation } from '../models/conversation.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ConversationService {
  http: HttpClient;

  constructor(http: HttpClient) {
    this.http = http;
  }

  getMyConversations() {
    return this.http.get<Conversation[]>(
      environment.apiUrl + '/conversations/my',
    );
  }
}

