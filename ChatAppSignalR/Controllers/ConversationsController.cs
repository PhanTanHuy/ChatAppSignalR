using System.Security.Claims;
using ChatAppSignalR.DTOs;
using ChatAppSignalR.Hubs;
using ChatAppSignalR.Models;
using ChatAppSignalR.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppSignalR.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly ConversationService _conversationService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ConversationsController(
            ConversationService conversationService,
            IHubContext<ChatHub> hubContext)
        {
            _conversationService = conversationService;
            _hubContext = hubContext;
        }

        [HttpGet("{conversationId}/messages")]
        public async Task<ActionResult<CursorPagedResponse<MessageResponse>>> GetMessages(
            string conversationId,
            [FromQuery] ConversationMessagesCursorQuery query)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            var conversation = await _conversationService.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                return NotFound(new { message = "Conversation không tồn tại" });
            }

            var isParticipant = _conversationService.IsParticipant(conversation, userId);
            if (!isParticipant)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Không phải thành viên của conversation" });
            }

            var result = await _conversationService.GetMessagesByCursorAsync(
                conversationId,
                query.Cursor,
                query.Limit
            );

            return Ok(result);
        }

        [HttpPatch("{conversationId}/seen")]
        public async Task<ActionResult<ApiMessageResponse>> MarkSeen(string conversationId)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            var conversation = await _conversationService.GetByIdAsync(conversationId);

            if (conversation == null)
            {
                return NotFound(new { message = "Conversation không tồn tại" });
            }

            if (!_conversationService.IsParticipant(conversation, userId))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    new { message = "Không phải thành viên của conversation" });
            }

            await _conversationService.MarkSeenAsync(conversationId, userId);

            var otherParticipantIds = _conversationService.GetOtherParticipantIds(conversation, userId);

            var payload = new
            {
                ConversationId = conversationId,
                SeenByUserId = userId
            };

            foreach (var participantId in otherParticipantIds)
            {
                await _hubContext.Clients.User(participantId)
                    .SendAsync("conversation_seen", payload);
            }

            return Ok(new ApiMessageResponse
            {
                Message = "Đã đánh dấu đã đọc"
            });
        }
        // add member to group conversation
        [HttpPost("{conversationId}/add-member")]
        public async Task<ActionResult<ConversationResponse>> AddMember(
            string conversationId,
            [FromBody] AddMemberRequest request)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            try
            {
                var conversation = await _conversationService.AddParticipantAsync(
                    conversationId,
                    request.UserId,
                    userId
                );

                return Ok(conversation);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
   
        /// remove member from group conversation
        [HttpDelete("{conversationId}/remove-member/{userId}")]
        public async Task<ActionResult<ApiMessageResponse>> RemoveMember(
            string conversationId,
            [FromRoute] RemoveMemberRequest request)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            try
            {
                await _conversationService.RemoveParticipantAsync(
                    conversationId,
                    request.UserId,
                    userId
                );

                return Ok(new ApiMessageResponse
                {
                    Message = "Đã xóa thành viên khỏi nhóm"
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ConversationResponse>> CreateConversation(CreateConversationRequest request)
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            try
            {
                var conversation = await _conversationService.CreateConversationAsync(request, userId);
                return Ok(new { conversation });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConversationResponse>>> GetUserConversations()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { message = "Chưa đăng nhập" });
            }

            var conversations = await _conversationService.GetUserConversationsAsync(userId);
            return Ok(conversations);
        }
    }
}