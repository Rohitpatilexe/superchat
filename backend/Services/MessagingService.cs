using backend.Config;
using backend.DTOs; // Assuming you create these DTOs
using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace backend.Services
{
    public class MessagingService
    {
        private readonly ApplicationDbContext _context;

        public MessagingService(ApplicationDbContext context)
        {
            _context = context;
        }
        // Called by a Leader to initiate a new conversation with a Vendor for a specific Job.
        public async Task<Conversation?> StartConversationAsync(Guid jobPublicId, Guid vendorPublicId, int leaderUserId)
        {
            // Step 1: Find all the necessary entities from the database.
            var job = await _context.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.PublicId == jobPublicId);
            var vendor = await _context.Vendors.AsNoTracking().FirstOrDefaultAsync(v => v.PublicId == vendorPublicId);

            if (job == null || vendor == null) return null; // Job or Vendor doesn't exist.

            // Step 2: Perform Authorization. A leader can only start a chat if:
            // a) They are the one who created the job.
            // b) The vendor is actually assigned to that job.
            bool isJobCreator = job.CreatedByLeaderId == leaderUserId;
            bool isVendorAssigned = await _context.JobVendors
                .AnyAsync(jv => jv.JobId == job.Id && jv.VendorId == vendor.Id);

            if (!isJobCreator || !isVendorAssigned)
            {
                // If either check fails, the action is forbidden.
                return null;
            }

            // Step 3: Check if a conversation already exists to prevent duplicates.
            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.JobId == job.Id && c.VendorId == vendor.Id);

            if (existingConversation != null)
            {
                return existingConversation; // Return the existing chat if found.
            }

            // Step 4: All checks passed. Create the new conversation record.
            var newConversation = new Conversation
            {
                JobId = job.Id,
                VendorId = vendor.Id,
                LeaderId = leaderUserId
            };

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

            return newConversation;
        }

        // Saves a new message to the database. Called by the ChatHub.
        public async Task<(MessageDto? newMessage, Guid? recipientId)> SaveAndGetRecipientAsync(Guid conversationPublicId, Guid senderPublicId, string content)
        {
            // Step 1: Fetch the conversation and its participants in a single query.
            var conversation = await _context.Conversations
                .Include(c => c.Leader) // The job creator
                .Include(c => c.Vendor).ThenInclude(v => v.User) // The vendor's user account
                .FirstOrDefaultAsync(c => c.PublicId == conversationPublicId);

            if (conversation?.Vendor.User == null) return (null, null); // Conversation or vendor user not found.

            // Step 2: Authorization: Verify the sender is one of the two participants in this chat.
            bool isParticipant = conversation.Leader.PublicId == senderPublicId || conversation.Vendor.User.PublicId == senderPublicId;
            if (!isParticipant)
            {
                // If the sender isn't part of this chat, they can't send a message.
                return (null, null);
            }

            // Step 3: Determine the recipient for the real-time push.
            var recipientPublicId = conversation.Leader.PublicId == senderPublicId
                ? conversation.Vendor.User.PublicId
                : conversation.Leader.PublicId;

            // Step 4: Create and save the message entity to the database.
            var message = new Message
            {
                ConversationId = conversation.Id,
                SenderId = senderPublicId == conversation.Leader.PublicId ? conversation.Leader.Id : conversation.Vendor.UserId.Value,
                Content = content
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Step 5: Map the new message to a DTO to be sent to the clients.
            var messageDto = new MessageDto(
                message.Id,
                message.Content,
                message.SentAt,
                senderPublicId,
                senderPublicId == conversation.Leader.PublicId ? $"{conversation.Leader.FirstName}" : conversation.Vendor.CompanyName
            );

            return (messageDto, recipientPublicId);
        }

        // Gets the message history for a given conversation.
        public async Task<IEnumerable<MessageDto>?> GetMessagesAsync(Guid conversationPublicId, Guid userPublicId)
        {
            var conversation = await _context.Conversations
                .AsNoTracking()
                .Include(c => c.Leader)
                .Include(c => c.Vendor).ThenInclude(v => v.User)
                .FirstOrDefaultAsync(c => c.PublicId == conversationPublicId);

            if (conversation?.Vendor.User == null) return null;

            // Authorization: Only a participant can view the messages.
            bool isParticipant = conversation.Leader.PublicId == userPublicId || conversation.Vendor.User.PublicId == userPublicId;
            if (!isParticipant) return null;

            // Fetch all messages, order them by time, and project to DTOs.
            return await _context.Messages
                .Where(m => m.ConversationId == conversation.Id)
                .OrderBy(m => m.SentAt)
                .Include(m => m.Sender) // To get sender details
                .Select(m => new MessageDto(
                    m.Id,
                    m.Content,
                    m.SentAt,
                    m.Sender.PublicId,
                    m.Sender.Id == conversation.LeaderId ? $"{m.Sender.FirstName}" : conversation.Vendor.CompanyName
                 ))
                .ToListAsync();
        }

        /// Gets a list of all conversations for a specific user.
        /// The DTO is shaped to be easily consumed by a chat list UI.
        /// <param name="userPublicId">The PublicId of the user requesting their conversations.</param>
        /// <returns>A list of ConversationDto objects.</returns>
        public async Task<IEnumerable<ConversationDto>> GetConversationListAsync(Guid userPublicId)
        {
            // The query finds all conversations where the user is either the Leader OR the Vendor.
            var conversations = await _context.Conversations
                .AsNoTracking()
                .Include(c => c.Leader)
                .Include(c => c.Vendor).ThenInclude(v => v.User)
                .Include(c => c.Job)
                .Include(c => c.Messages) // Include messages to find the last one
                .Where(c => c.Leader.PublicId == userPublicId || c.Vendor.User.PublicId == userPublicId)
                .OrderByDescending(c => c.Messages.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt) // Order by last message time
                .ToListAsync();

            // The database query gets all the data. Now we shape it into the DTOs in memory.
            // This is easier than doing it in a single complex EF Core query.
            return conversations.Select(c =>
            {
                // Find the most recent message for the preview.
                var lastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();

                // Determine who the *other* person in the chat is.
                var isUserTheLeader = c.Leader.PublicId == userPublicId;
                var participantName = isUserTheLeader ? c.Vendor.CompanyName : $"{c.Leader.FirstName} {c.Leader.LastName}";
                var participantPublicId = isUserTheLeader ? c.Vendor.User.PublicId : c.Leader.PublicId;

                return new ConversationDto(
                    c.PublicId,
                    c.Job.Title,
                    participantPublicId,
                    participantName,
                    lastMessage?.Content, // Use null-conditional operator in case there are no messages
                    lastMessage?.SentAt
                );
            });
        }
    }
}