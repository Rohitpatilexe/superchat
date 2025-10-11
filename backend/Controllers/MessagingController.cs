using backend.DTOs;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/messaging")]
    public class MessagingController : ControllerBase
    {
        private readonly MessagingService _messagingService;

        public MessagingController(MessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        private Guid GetCurrentUserPublicId() => Guid.Parse(User.FindFirstValue("PublicId"));

        // Endpoint for a Leader to start a conversation with a Vendor about a Job.
        [HttpPost("conversations")]
        [Authorize(Roles = "Leadership")]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest dto)
        {
            var conversation = await _messagingService.StartConversationAsync(dto.JobPublicId, dto.VendorPublicId, GetCurrentUserId());

            // If the service returns null, it means the user was not authorized to perform this action.
            if (conversation == null)
            {
                return Forbid();
            }

            return Ok(new { conversationPublicId = conversation.PublicId });
        }

        // To get the message history for a specific conversation.
        [Authorize]
        [HttpGet("conversations/{publicId:guid}/messages")]
        public async Task<IActionResult> GetMessages(Guid publicId)
        {
            var messages = await _messagingService.GetMessagesAsync(publicId, GetCurrentUserPublicId());

            // If the service returns null, the user is not a participant of this conversation.
            if (messages == null)
            {
                return Forbid();
            }

            return Ok(messages);
        }

        // Gets a list of all conversations the currently authenticated user is a part of.
        [Authorize]
        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            // Get the current user's ID from their JWT claims.
            var currentUserPublicId = GetCurrentUserPublicId();

            // The service layer handles all the complex logic.
            var conversations = await _messagingService.GetConversationListAsync(currentUserPublicId);

            return Ok(conversations);
        }
    }
}