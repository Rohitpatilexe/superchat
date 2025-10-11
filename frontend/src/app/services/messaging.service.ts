import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import { MessageDto, ConversationDto } from '../models';

@Injectable({
  providedIn: 'root'
})
export class MessagingService {
  private apiUrl = `${environment.apiUrl}/messaging`;
  private hubConnection!: signalR.HubConnection;

  // BehaviorSubjects act as observable streams that components can subscribe to for real-time updates.
  public messages$ = new BehaviorSubject<MessageDto[]>([]);
  public conversations$ = new BehaviorSubject<ConversationDto[]>([]);

  constructor(private http: HttpClient) { }

  // --- Real-time SignalR Connection Management ---

  /**
   * Starts and configures the SignalR connection.
   * This should be called once the user is authenticated.
   */
  public startConnection(): void {
    // Prevent multiple connections
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = localStorage.getItem('token');
    if (!token) {
      console.error('SignalR connection cannot start without an auth token.');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl.replace('/api', '')}/chatHub`, {
        accessTokenFactory: () => token // The token is sent for authentication
      })
      .withAutomaticReconnect() // Automatically try to reconnect if the connection is lost
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR Connection started successfully.');
        this.registerServerEvents();
      })
      .catch(err => console.error('Error while starting SignalR connection: ' + err));
  }

  /**
   * Stops the SignalR connection. This should be called on user logout.
   */
  public stopConnection(): void {
    if (this.hubConnection) {
      this.hubConnection.stop()
        .then(() => console.log('SignalR Connection stopped.'))
        .catch(err => console.error('Error while stopping SignalR connection: ' + err));
    }
  }

  /**
   * Central place to register listeners for events pushed from the server hub.
   */
  private registerServerEvents(): void {
    // Listen for the "ReceiveMessage" event from the server
    this.hubConnection.on('ReceiveMessage', (newMessage: MessageDto) => {
      // When a new message arrives, add it to our local message stream
      const currentMessages = this.messages$.getValue();
      this.messages$.next([...currentMessages, newMessage]);
    });
  }

  /**
   * Sends a message to a conversation via the hub.
   * @param conversationPublicId The ID of the conversation.
   * @param content The text of the message.
   */
  public sendMessage(conversationPublicId: string, content: string): Promise<void> {
    // 'SendMessage' must match the method name in your .NET ChatHub class
    return this.hubConnection.invoke('SendMessage', conversationPublicId, content);
  }

  // --- REST API Methods for managing conversations and history ---

  /**
   * Fetches the user's list of all their conversations.
   * Also updates the conversations$ stream.
   */
  getMyConversations(): Observable<ConversationDto[]> {
    return this.http.get<ConversationDto[]>(`${this.apiUrl}/conversations`).pipe(
      tap(conversations => this.conversations$.next(conversations))
    );
  }

  /**
   * Fetches the message history for a specific conversation.
   * Also updates the messages$ stream with the history.
   */
  getMessages(conversationPublicId: string): Observable<MessageDto[]> {
    // Clear old messages before loading new ones
    this.messages$.next([]);
    return this.http.get<MessageDto[]>(`${this.apiUrl}/conversations/${conversationPublicId}/messages`).pipe(
      tap(messages => this.messages$.next(messages))
    );
  }
  
  /**
   * Calls the API to start a new conversation. (Leader only)
   * @returns An observable with the new conversation's PublicId.
   */
  startConversation(jobPublicId: string, vendorPublicId: string): Observable<{ conversationPublicId: string }> {
    return this.http.post<{ conversationPublicId: string }>(`${this.apiUrl}/conversations`, { jobPublicId, vendorPublicId });
  }
}