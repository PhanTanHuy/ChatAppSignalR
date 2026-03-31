using ChatAppSignalR.Data;
using ChatAppSignalR.DTOs;
using ChatAppSignalR.Models;
using MongoDB.Driver;

namespace ChatAppSignalR.Services
{
    public class ConversationService
    {
        private readonly MongoDbContext _context;

        public ConversationService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Conversation?> GetByIdAsync(string conversationId)
        {
            return await _context.Conversations
                .Find(c => c.Id == conversationId)
                .FirstOrDefaultAsync();
        }

        public bool IsParticipant(Conversation conversation, string userId)
        {
            return conversation.ParticipantIds.Contains(userId);
        }

        public async Task<CursorPagedResponse<MessageResponse>> GetMessagesByCursorAsync(
            string conversationId,
            DateTime? cursor,
            int limit)
        {
            var filterBuilder = Builders<Message>.Filter;

            var filter = filterBuilder.Eq(m => m.ConversationId, conversationId);

            if (cursor.HasValue)
            {
                filter = filterBuilder.And(
                    filter,
                    filterBuilder.Lt(m => m.CreatedAt, cursor.Value)
                );
            }

            var messages = await _context.Messages
                .Find(filter)
                .SortByDescending(m => m.CreatedAt)
                .Limit(limit + 1)
                .ToListAsync();

            var hasMore = messages.Count > limit;

            if (hasMore)
            {
                messages = messages.Take(limit).ToList();
            }

            var senderIds = messages
                .Select(m => m.SenderId)
                .Distinct()
                .ToList();

            var users = await _context.Users
                .Find(u => senderIds.Contains(u.Id))
                .ToListAsync();

            var userMap = users.ToDictionary(
                u => u.Id,
                u => new SenderInfoDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    AvatarUrl = u.AvatarUrl
                });

            var items = messages.Select(m => new MessageResponse
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                Content = m.Content,
                ImgUrl = m.ImgUrl,
                CreatedAt = m.CreatedAt,
                Sender = userMap.TryGetValue(m.SenderId, out var sender) ? sender : null
            }).ToList();

            DateTime? nextCursor = null;

            if (items.Count > 0)
            {
                nextCursor = items.Last().CreatedAt;
            }

            return new CursorPagedResponse<MessageResponse>
            {
                Items = items,
                NextCursor = nextCursor,
                HasMore = hasMore,
                Limit = limit
            };
        }

        public async Task MarkSeenAsync(string conversationId, string userId)
        {
            var update = Builders<Conversation>.Update
                .AddToSet(c => c.SeenBy, userId)
                .Set($"UnreadCounts.{userId}", 0);

            await _context.Conversations.UpdateOneAsync(
                c => c.Id == conversationId,
                update
            );
        }

        public List<string> GetOtherParticipantIds(Conversation conversation, string currentUserId)
        {
            return conversation.ParticipantIds
                .Where(id => id != currentUserId)
                .ToList();
        }
    }
}