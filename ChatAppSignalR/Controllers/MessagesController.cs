using System.Security.Claims;
using ChatAppSignalR.DTOs;
using ChatAppSignalR.Hubs;
using ChatAppSignalR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppSignalR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly MessageService _messageService;
        private readonly ConversationService _conversationService;
        private readonly IHubContext<ChatHub> _hubContext;

        public MessagesController(
            MessageService messageService,
            ConversationService conversationService,
            IHubContext<ChatHub> hubContext)
        {
            _messageService = messageService;
            _conversationService = conversationService;
            _hubContext = hubContext;
        }

        [HttpPost("direct")]
        public async Task<ActionResult<MessageResponse>> SendDirect([FromBody] SendDirectMessageRequest request)
        {
            var senderId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(senderId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            if (senderId == request.RecipientId)
            {
                return BadRequest(new { message = "Không thể tự gửi tin nhắn cho chính mình" });
            }

            ChatAppSignalR.Models.Conversation? conversation;

            if (!string.IsNullOrWhiteSpace(request.ConversationId))
            {
                conversation = await _messageService.GetConversationByIdAsync(request.ConversationId!);

                if (conversation == null)
                {
                    return BadRequest(new { message = "Conversation không tồn tại" });
                }

                var isParticipant = conversation.ParticipantIds.Contains(senderId) &&
                                    conversation.ParticipantIds.Contains(request.RecipientId);

                if (!isParticipant)
                {
                    return BadRequest(new { message = "Conversation không hợp lệ cho 2 user này" });
                }
            }
            else
            {
                conversation = await _messageService.FindDirectConversationAsync(senderId, request.RecipientId);

                if (conversation == null)
                {
                    conversation = await _messageService.CreateDirectConversationAsync(senderId, request.RecipientId);
                }
            }

            var message = await _messageService.CreateMessageAsync(
                conversation.Id,
                senderId,
                request.Content,
                request.ImgUrl
            );

            await _messageService.UpdateConversationAfterSendAsync(
                conversation,
                message,
                senderId,
                request.RecipientId
            );

            var response = MessageService.ToResponse(message);

            // Build full ConversationResponse (includes lastMessage with Sender info)
            var conversationResponse = await _conversationService.GetConversationResponseAsync(conversation.Id);

            var payload = new
            {
                message = response,
                conversation = conversationResponse,
                unreadCounts = conversation.UnreadCounts
            };

            await _hubContext.Clients.User(request.RecipientId)
                .SendAsync("new-message", payload);

            return Created(string.Empty, response);
        }
    }
}