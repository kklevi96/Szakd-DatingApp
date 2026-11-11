import {
  afterNextRender,
  Component,
  DestroyRef,
  DOCUMENT,
  ElementRef,
  HostListener,
  Inject,
  Injector,
  Input,
  OnChanges, OnInit,
  signal,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { SignalRService } from '../../../core/services/signalr.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { finalize, Subscription, } from 'rxjs';
import { Message, SendMessageRequest, } from '../../../core/models/message.models';
import { MessageService } from '../../../core/services/message-service';
import { ConversationService } from '../../../core/services/conversation-service';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth-service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule, } from '@angular/material/snack-bar';

@Component({
  selector: 'app-messages',
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatSnackBarModule,
  ],
  templateUrl: './messages.html',
  styleUrl: './messages.scss',
})
export class Messages implements OnChanges, OnInit {
  @Input({ required: true }) conversationId = 0;
  @Input() isActive = true;

  messages = signal<Message[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  newMessage = new SendMessageRequest();
  isSending = signal(false);

  private messagesRequest?: Subscription;
  private readRequest?: Subscription;
  private lastReadMessageId = 0;
  private loadedToken: string | null = null;

  @ViewChild('messageList')
  private messageList?: ElementRef<HTMLDivElement>;

  constructor(
    private messageService: MessageService,
    private destroyRef: DestroyRef,
    private injector: Injector,
    public auth: AuthService,
    private snackBar: MatSnackBar,
    private signalRService: SignalRService,
    private conversationService: ConversationService,
    @Inject(DOCUMENT) private document: Document,
  ) {}

  ngOnInit(): void {
    this.signalRService.messagesChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(conversationId => {
        if (!this.auth.getToken()) {
          return;
        }

        if (
          conversationId === null ||
          conversationId === this.conversationId
        ) {
          this.getMessages();
        }
      });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['conversationId']) {
      this.readRequest?.unsubscribe();
      this.lastReadMessageId = 0;
      this.loadedToken = null;
      this.newMessage = new SendMessageRequest();
      this.messages.set([]);
      this.getMessages();
    } else if (changes['isActive'] && this.isActive) {
      this.scrollToBottom();
    }
  }

  getMessages(): void {
    this.messagesRequest?.unsubscribe();
    const conversationId = this.conversationId;
    const token = this.auth.getToken();
    if (!conversationId || !token) { return; }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.messagesRequest = this.messageService
      .getMessages(conversationId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          if (this.conversationId !== conversationId || this.auth.getToken() !== token) { return; }
          if (this.loadedToken !== token) {
            this.readRequest?.unsubscribe();
            this.lastReadMessageId = 0;
          }
          this.loadedToken = token;
          this.messages.set(response);
          this.isLoading.set(false);

          this.scrollToBottom();
        },
        error: () => {
          if (this.conversationId !== conversationId || this.auth.getToken() !== token) { return; }
          this.errorMessage.set('Error when loading messages, try again!');
          this.isLoading.set(false);
        },
      });
  }

  sendMessage(): void {
    if (this.isSending() || this.isLoading() || this.errorMessage() || !this.newMessage.content.trim()) {
      return;
    }

    const token = this.auth.getToken();
    if (!token) { return; }

    const conversationId = this.conversationId;
    this.newMessage.conversationId = conversationId;
    this.newMessage.content = this.newMessage.content.trim();
    this.isSending.set(true);

    this.messageService.sendMessage(this.newMessage).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.isSending.set(false)),
    ).subscribe({
      next: () => {
        if (this.conversationId !== conversationId || this.auth.getToken() !== token) { return; }
        this.newMessage = new SendMessageRequest();
        this.getMessages();
      },
      error: () => {
        if (this.conversationId !== conversationId || this.auth.getToken() !== token) { return; }
        this.snackBar.open('Message could not be sent. Please try again.', 'Close', {
          duration: 5000,
        });
      },
    });
  }

  scrollToBottom(): void {
    const conversationId = this.conversationId;
    const token = this.auth.getToken();
    afterNextRender(() => {
      if (this.destroyRef.destroyed || this.conversationId !== conversationId || this.auth.getToken() !== token) { return; }
      const list = this.messageList?.nativeElement;
      if (list) {
        list.scrollTop = list.scrollHeight;
        this.markAsRead();
      }
    }, { injector: this.injector });
  }

  @HostListener('document:visibilitychange')
  @HostListener('window:focus')
  onViewVisible(): void {
    if (this.isActive && this.document.visibilityState === 'visible' && this.document.hasFocus()) {
      this.scrollToBottom();
    }
  }

  private markAsRead(): void {
    const token = this.auth.getToken();
    if (!token || this.loadedToken !== token || !this.isActive || this.isLoading() || this.errorMessage() ||
      this.document.visibilityState !== 'visible' || !this.document.hasFocus() ||
      (this.readRequest && !this.readRequest.closed)) {
      return;
    }

    const lastReadMessageId = this.messages().reduce((lastId, message) => Math.max(lastId, message.id), 0);
    if (lastReadMessageId <= this.lastReadMessageId) { return; }

    const conversationId = this.conversationId;
    this.readRequest = this.conversationService.markAsRead(conversationId, lastReadMessageId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          if (this.conversationId !== conversationId || this.auth.getToken() !== token) { return; }
          this.lastReadMessageId = lastReadMessageId;
          this.scrollToBottom();
        },
        error: () => {},
      });
  }
}
