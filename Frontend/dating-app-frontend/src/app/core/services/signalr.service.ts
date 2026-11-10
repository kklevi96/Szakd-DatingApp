import { Injectable, OnDestroy, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { ConversationNameChange } from '../models/conversation.models';
import { environment } from '../../../environments/environment';

type ConnectionState = 'disconnected' | 'connecting' | 'connected' | 'reconnecting';

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private hubConnection?: signalR.HubConnection;
  private disconnectsFor?: signalR.HubConnection;
  private token: string | null = null;
  private retryTimer?: ReturnType<typeof setTimeout>;
  private state = signal<ConnectionState>('disconnected');
  readonly connectionState = this.state.asReadonly();

  private matchesChanged = new Subject<void>();
  readonly matchesChanged$ = this.matchesChanged.asObservable();

  private messagesChanged = new Subject<number | null>();
  readonly messagesChanged$ = this.messagesChanged.asObservable();

  private potentialFriendsChanged = new Subject<void>();
  readonly potentialFriendsChanged$ = this.potentialFriendsChanged.asObservable();

  private conversationsChanged = new Subject<void>();
  readonly conversationsChanged$ = this.conversationsChanged.asObservable();

  private conversationRenamed = new Subject<ConversationNameChange | null>();
  readonly conversationRenamed$ = this.conversationRenamed.asObservable();

  public startConnection = (token: string): void => {
    if (!token) {
      this.stopConnection();
      return;
    }
    if (token === this.token && this.hubConnection) {
      return;
    }

    this.stopConnection();
    this.token = token;
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build();

    this.addMatchesListener();
    this.addPotentialFriendsListener();
    this.addMessagesListener();
    this.addConversationRenamedListener();
    this.addConversationsListener();

    this.handleDisconnects();
    this.connect(this.hubConnection);
  };

  public addMatchesListener = (): void => {
    const connection = this.hubConnection;
    if (!connection) {
      return;
    }

    connection.off('MatchesChanged');
    connection.on('MatchesChanged', () => {
      if (this.hubConnection === connection) {
        this.matchesChanged.next();
      }
    });
  };

  public addPotentialFriendsListener = (): void => {
    const connection = this.hubConnection;

    if (!connection) {
      return;
    }

    connection.off('PotentialFriendsChanged');
    connection.on('PotentialFriendsChanged', () => {
      if (this.hubConnection === connection) {
        this.potentialFriendsChanged.next();
      }
    });
  };

  public addMessagesListener = (): void => {
    const connection = this.hubConnection;
    if (!connection) {
      return;
    }

    connection.off('MessagesChanged');
    connection.on('MessagesChanged', (conversationId: number) => {
      if (this.hubConnection === connection) {
        this.messagesChanged.next(conversationId);
      }
    });
  };

  public addConversationRenamedListener = (): void => {
    const connection = this.hubConnection;
    if (!connection) {
      return;
    }

    connection.off('ConversationRenamed');
    connection.on('ConversationRenamed', (change: ConversationNameChange) => {
      if (this.hubConnection === connection) {
        this.conversationRenamed.next(change);
      }
    });
  };

  public addConversationsListener = (): void => {
    const connection = this.hubConnection;
    if (!connection) {
      return;
    }

    connection.off('ConversationsChanged');
    connection.on('ConversationsChanged', () => {
      if (this.hubConnection === connection) {
        this.conversationsChanged.next();
      }
    });
  };

  public handleDisconnects = (): void => {
    const connection = this.hubConnection;
    if (!connection || this.disconnectsFor === connection) {
      return;
    }
    this.disconnectsFor = connection;

    connection.onreconnecting(() => {
      if (this.hubConnection === connection) {
        this.state.set('reconnecting');
      }
    });

    connection.onreconnected(() => {
      if (this.hubConnection !== connection) {
        return;
      }
      this.state.set('connected');
      this.matchesChanged.next();
      this.potentialFriendsChanged.next();
      this.messagesChanged.next(null);
      this.conversationsChanged.next();
    });

    connection.onclose(() => {
      if (this.hubConnection !== connection) {
        return;
      }
      this.state.set('disconnected');
      this.scheduleRetry(connection);
    });
  };

  public stopConnection = (): void => {
    this.token = null;
    this.clearRetry();
    const connection = this.hubConnection;
    this.hubConnection = undefined;
    this.disconnectsFor = undefined;
    this.state.set('disconnected');

    if (connection) {
      void connection.stop().catch(() => {});
    }
  };

  private connect(connection: signalR.HubConnection): void {
    if (
      this.hubConnection !== connection ||
      connection.state !== signalR.HubConnectionState.Disconnected
    ) {
      return;
    }

    this.state.set('connecting');
    connection
      .start()
      .then(() => {
        if (this.hubConnection !== connection) {
          void connection.stop().catch(() => {});
          return;
        }

        this.state.set('connected');
        this.matchesChanged.next();
        this.potentialFriendsChanged.next();
        this.messagesChanged.next(null);
        this.conversationsChanged.next();
      })
      .catch(() => {
        if (this.hubConnection !== connection) {
          return;
        }
        this.state.set('disconnected');
        this.scheduleRetry(connection);
      });
  }

  private scheduleRetry(connection: signalR.HubConnection): void {
    if (this.retryTimer !== undefined || this.hubConnection !== connection) {
      return;
    }

    this.retryTimer = setTimeout(() => {
      this.retryTimer = undefined;
      this.connect(connection);
    }, 5000);
  }

  private clearRetry(): void {
    if (this.retryTimer !== undefined) {
      clearTimeout(this.retryTimer);
      this.retryTimer = undefined;
    }
  }

  ngOnDestroy(): void {
    this.stopConnection();
    this.matchesChanged.complete();
    this.potentialFriendsChanged.complete();
    this.messagesChanged.complete();
    this.conversationRenamed.complete();
    this.conversationsChanged.complete();
  }
}
