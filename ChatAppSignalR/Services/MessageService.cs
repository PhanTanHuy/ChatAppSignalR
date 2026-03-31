using ChatAppSignalR.Data;
using ChatAppSignalR.DTOs;
using ChatAppSignalR.Models;
using MongoDB.Driver;

namespace ChatAppSignalR.Services
{
    public class MessageService
    {
        private readonly MongoDbContext _context;

        public MessageService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AreFriendsAsync(string userId, string otherUserId)
        {
            var direct = await _context.Friends
                .Find(f => f.UserId == userId && f.FriendUserId == otherUserId)
                .AnyAsync();

            var reverse = await _context.Friends
                .Find(f => f.UserId == otherUserId && f.FriendUserId == userId)
                .AnyAsync();

            return direct || reverse;
        }

        public async Task<Conversation?> GetConversationByIdAsync(string conversationId)
        {
            return await _context.Conversations
                .Find(c => c.Id == conversationId)
                .FirstOrDefaultAsync();
        }

        public async Task<Conversation?> FindDirectConversationAsync(string userId1, string userId2)
        {
            return await _context.Conversations
                .Find(c =>
                    c.IsDirect &&
                    c.ParticipantIds.Count == 2 &&
                    c.ParticipantIds.Contains(userId1) &&
                    c.ParticipantIds.Contains(userId2))
                .FirstOrDefaultAsync();
        }

        public async Task<Conversation> CreateDirectConversationAsync(string senderId, string recipientId)
        {
            var conversation = new Conversation
            {
                ParticipantIds = new List<string> { senderId, recipientId },
                IsDirect = true,
                SeenBy = new List<string>(),
                UnreadCounts = new Dictionary<string, int>
                {
                    [senderId] = 0,
                    [recipientId] = 0
                },
                CreatedAt = DateTime.UtcNow
            };

            await _context.Conversations.InsertOneAsync(conversation);
            return conversation;
        }

        public async Task<Message> CreateMessageAsync(string conversationId, string senderId, string? content, string? imgUrl)
        {
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = string.IsNullOrWhiteSpace(content) ? null : content.Trim(),
                ImgUrl = string.IsNullOrWhiteSpace(imgUrl) ? null : imgUrl.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Messages.InsertOneAsync(message);
            return message;
        }

        public async Task UpdateConversationAfterSendAsync(Conversation conversation, Message message, string senderId, string recipientId)
        {
            if (!conversation.UnreadCounts.ContainsKey(senderId))
                conversation.UnreadCounts[senderId] = 0;

            if (!conversation.UnreadCounts.ContainsKey(recipientId))
                conversation.UnreadCounts[recipientId] = 0;

            conversation.SeenBy = new List<string>();
            conversation.UnreadCounts[senderId] = 0;
            conversation.UnreadCounts[recipientId] += 1;
            conversation.LastMessageId = message.Id;
            conversation.LastMessageAt = message.CreatedAt;

            var update = Builders<Conversation>.Update
                .Set(c => c.SeenBy, conversation.SeenBy)
                .Set(c => c.UnreadCounts, conversation.UnreadCounts)
                .Set(c => c.LastMessageId, conversation.LastMessageId)
                .Set(c => c.LastMessageAt, conversation.LastMessageAt);

            await _context.Conversations.UpdateOneAsync(c => c.Id == conversation.Id, update);
        }

        public static MessageResponse ToResponse(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                Content = message.Content,
                ImgUrl = message.ImgUrl,
                CreatedAt = message.CreatedAt
            };
        }
    }
}