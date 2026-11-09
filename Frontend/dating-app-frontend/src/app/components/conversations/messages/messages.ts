import {
  afterNextRender,
  Component,
  DestroyRef,
  ElementRef,
  Injector,
  Input,
  OnChanges,
  signal,
  SimpleChanges,
  ViewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { finalize, Subscription } from 'rxjs';
import { Message, SendMessageRequest } from '../../../core/models/message.models';
import { MessageService } from '../../../core/services/message-service';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/services/auth-service';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

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
export class Messages implements OnChanges {
  @Input({ required: true }) conversationId = 0;

  messages = signal<Message[]>([]);
  isLoading = signal(false);
  errorMessage = signal('');
  newMessage = new SendMessageRequest();
  isSending = signal(false);

  private messagesRequest?: Subscription;

  @ViewChild('messageList')
  private messageList?: ElementRef<HTMLDivElement>;

  constructor(
    private messageService: MessageService,
    private destroyRef: DestroyRef,
    private injector: Injector,
    public auth: AuthService,
    private snackBar: MatSnackBar,
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['conversationId']) {
      this.newMessage = new SendMessageRequest();
      this.messages.set([]);
      this.getMessages();
    }
  }

  getMessages(): void {
    this.messagesRequest?.unsubscribe();
    this.isLoading.set(true);
    this.errorMessage.set('');

    this.messagesRequest = this.messageService
      .getMessages(this.conversationId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          this.messages.set(response);
          this.isLoading.set(false);

          this.scrollToBottom();
        },
        error: () => {
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

  private scrollToBottom(): void {
    afterNextRender(() => {
      const list = this.messageList?.nativeElement;
      if (list) { list.scrollTop = list.scrollHeight; }
    }, { injector: this.injector });
  }
}
