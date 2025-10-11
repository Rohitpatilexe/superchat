import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { MessagingService } from '../services/messaging.service';
import { AuthService } from '../services/auth.service';
import { ConversationDto, MessageDto } from '../models';

@Component({
  selector: 'app-chat-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  template: `
    <div class="fixed inset-0 bg-black bg-opacity-50 z-40" (click)="close()"></div>

    <div class="fixed bottom-0 right-0 md:right-4 md:bottom-4 w-full h-full md:w-96 md:h-[600px] bg-white rounded-t-lg md:rounded-lg shadow-2xl z-50 flex flex-col">
      <div class="flex justify-between items-center p-3 border-b bg-gray-50 rounded-t-lg">
        <h3 class="font-bold text-lg text-gray-800">{{ conversation.participantName }}</h3>
        <button (click)="close()" class="text-gray-500 hover:text-gray-800 text-2xl leading-none">&times;</button>
      </div>

      <div #messageContainer class="flex-1 p-4 overflow-y-auto space-y-4">
        <div *ngIf="isLoading" class="text-center text-gray-500">Loading messages...</div>
        <div *ngFor="let msg of messages" class="flex"
             [ngClass]="{'justify-end': msg.senderPublicId === myPublicId, 'justify-start': msg.senderPublicId !== myPublicId}">
          <div class="max-w-xs lg:max-w-md px-3 py-2 rounded-lg"
               [ngClass]="{'bg-blue-600 text-white': msg.senderPublicId === myPublicId, 'bg-gray-200 text-gray-800': msg.senderPublicId !== myPublicId}">
            <p class="text-sm">{{ msg.content }}</p>
            <p class="text-xs opacity-75 mt-1 text-right">{{ msg.sentAt | date:'shortTime' }}</p>
          </div>
        </div>
      </div>

      <div class="p-3 border-t bg-gray-50 rounded-b-lg">
        <form (ngSubmit)="sendMessage()" class="flex items-center">
          <textarea [(ngModel)]="newMessageContent" name="message"
                    (keydown.enter)="sendMessage(); $event.preventDefault()"
                    placeholder="Type your message..."
                    class="flex-1 p-2 border rounded-md resize-none" rows="1"></textarea>
          <button type="submit" [disabled]="!newMessageContent.trim()"
                  class="ml-2 px-4 py-2 text-white bg-blue-600 rounded-md disabled:bg-gray-400">Send</button>
        </form>
      </div>
    </div>
  `
})
export class ChatModalComponent implements OnInit, OnDestroy, AfterViewChecked {
  @Input() conversation!: ConversationDto;
  @Output() closeModal = new EventEmitter<void>();

  @ViewChild('messageContainer') private messageContainer!: ElementRef;

  messages: MessageDto[] = [];
  newMessageContent = '';
  myPublicId: string | null = null;
  isLoading = true;

  private messagesSubscription!: Subscription;

  constructor(
    private messagingService: MessagingService,
    private authService: AuthService
  ) {
    const decodedToken: any = this.authService.getDecodedToken();
    this.myPublicId = decodedToken?.PublicId || null;
  }

  ngOnInit(): void {
    this.messagesSubscription = this.messagingService.messages$.subscribe(messages => {
      this.messages = messages;
    });

    this.messagingService.getMessages(this.conversation.conversationPublicId).subscribe(() => {
      this.isLoading = false;
    });
  }

  ngOnDestroy(): void {
    if (this.messagesSubscription) {
      this.messagesSubscription.unsubscribe();
    }
  }
  
  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }

  sendMessage(): void {
    const content = this.newMessageContent.trim();
    if (!content) return;

    // --- FIX: OPTIMISTIC UPDATE ---
    // Create a temporary message object to display immediately.
    const optimisticMessage: MessageDto = {
      id: Date.now(), // Temporary ID
      content: content,
      sentAt: new Date().toISOString(),
      senderPublicId: this.myPublicId!,
      senderName: 'Me' // Placeholder name
    };
    // Add it to the local array right away.
    this.messages.push(optimisticMessage);
    this.newMessageContent = ''; // Clear input immediately
    this.scrollToBottom();
    // ----------------------------

    this.messagingService.sendMessage(this.conversation.conversationPublicId, content)
      .catch(err => {
        console.error('Error sending message:', err);
        // Optional: Implement logic to show a "failed to send" indicator
        this.messages.pop(); // Remove the optimistic message on failure
      });
  }


  close(): void {
    this.closeModal.emit();
  }
  
  private scrollToBottom(): void {
    try {
      this.messageContainer.nativeElement.scrollTop = this.messageContainer.nativeElement.scrollHeight;
    } catch (err) { /* Gracefully handle error if element isn't ready */ }
  }
}