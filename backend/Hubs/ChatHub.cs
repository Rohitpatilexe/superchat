using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace backend.Hubs
{
    [Authorize] // Ensures that only authenticated users can connect to this hub.
    public class ChatHub : Hub
    {
        private readonly MessagingService _messagingService;

        public ChatHub(MessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        /// <summary>
        /// Method called by a client to send a message to a conversation.
        /// </summary>
        public async Task SendMessage(Guid conversationPublicId, string content)
        {
            // Get the sender's identity from their authenticated connection.
            var senderPublicId = Guid.Parse(Context.User.FindFirstValue("PublicId"));

            // Delegate all the hard work (validation, saving, finding recipient) to the service.
            var (newMessage, recipientPublicId) = await _messagingService.SaveAndGetRecipientAsync(conversationPublicId, senderPublicId, content);

            // If the service processed the message successfully...
            if (newMessage != null && recipientPublicId.HasValue)
            {
                // ...push the new message in real-time to the recipient's private group.
                // The client-side will listen for the "ReceiveMessage" event.
                await Clients.Group(recipientPublicId.Value.ToString())
                             .SendAsync("ReceiveMessage", newMessage);
            }
        }

        /// <summary>
        /// Called automatically by SignalR when a new client connects.
        /// We add the user to a private group named after their own PublicId.
        /// This creates a secure "inbox" that we can use to send them direct messages.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userPublicId = Context.User.FindFirstValue("PublicId");
            if (userPublicId != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, userPublicId);
            }
            await base.OnConnectedAsync();
        }
    }
}