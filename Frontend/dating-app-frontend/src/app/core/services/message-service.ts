import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Message, SendMessageRequest } from '../models/message.models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class MessageService {
  http: HttpClient;

  constructor(http: HttpClient) {
    this.http = http;
  }

  getMessages(conversationId: number) {
    return this.http.get<Message[]>(
      environment.apiUrl + '/messages/conversation/' + conversationId,
    );
  }

  sendMessage(request: SendMessageRequest) {
    return this.http.post<Message>(
      environment.apiUrl + '/messages',
      request,
    );
  }
}
