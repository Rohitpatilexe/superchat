import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MessagingService } from '../services/messaging.service';
import { ConversationDto } from '../models';
import { ChatModalComponent } from './chat-modal.component';

@Component({
  standalone: true,
  imports: [CommonModule, RouterModule, DatePipe, ChatModalComponent],
  template: `
    <div class="p-6 bg-white rounded-lg shadow">
      <h2 class="text-2xl font-bold text-gray-800">My Conversations</h2>

      <div *ngIf="isLoading" class="mt-4 text-center"><p>Loading conversations...</p></div>
      <div *ngIf="!isLoading && !conversations.length" class="mt-4 p-4 text-center bg-gray-50 rounded-md">
        You do not have any active conversations.
      </div>

      <div class="mt-4 space-y-3">
        <div *ngFor="let convo of conversations" (click)="openChat(convo)"
           class="block p-4 border rounded-md hover:shadow-lg hover:border-blue-500 transition-all cursor-pointer">
          <div class="flex justify-between items-start">
            <div class="flex-grow">
              <p class="text-sm font-medium text-gray-500">Regarding Job:</p>
              <h3 class="font-semibold text-lg text-blue-700">{{ convo.jobTitle }}</h3>
              <p class="text-sm text-gray-800 mt-1">With: <span class="font-semibold">{{ convo.participantName }}</span></p>
            </div>
            <div *ngIf="convo.lastMessageTimestamp" class="text-right text-xs text-gray-500 flex-shrink-0 ml-4">
              <p>{{ convo.lastMessageTimestamp | date:'shortDate' }}</p>
              <p>{{ convo.lastMessageTimestamp | date:'shortTime' }}</p>
            </div>
          </div>
          <div *ngIf="convo.lastMessage" class="mt-2 pt-2 border-t border-gray-200">
            <p class="text-sm text-gray-600 italic truncate">
              "{{ convo.lastMessage }}"
            </p>
          </div>
        </div>
      </div>
    </div>

    <app-chat-modal *ngIf="selectedConversation" 
                    [conversation]="selectedConversation" 
                    (closeModal)="closeChat()">
    </app-chat-modal>
  `
})
export class ConversationsComponent implements OnInit {
  conversations: ConversationDto[] = [];
  isLoading = true;
  selectedConversation: ConversationDto | null = null;

  constructor(private messagingService: MessagingService) {}

  ngOnInit(): void {
    this.messagingService.getMyConversations().subscribe({
      next: (data) => {
        this.conversations = data;
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load conversations:', err);
        this.isLoading = false;
      }
    });
  }

  openChat(conversation: ConversationDto): void {
    this.selectedConversation = conversation;
  }

  closeChat(): void {
    this.selectedConversation = null;
  }
}