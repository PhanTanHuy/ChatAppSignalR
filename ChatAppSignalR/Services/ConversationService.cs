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

        public async Task<ConversationResponse> CreateConversationAsync(CreateConversationRequest request, string currentUserId)
        {
            if (request.IsDirect && request.ParticipantIds.Count == 1)
            {
                var existingConversation = await FindDirectConversationAsync(currentUserId, request.ParticipantIds[0]);
                if (existingConversation != null)
                {
                    return await ConvertToConversationResponseAsync(existingConversation, currentUserId);
                }
            }

            var conversation = new Conversation
            {
                IsDirect = request.IsDirect,
                ParticipantIds = new List<string> { currentUserId },
                CreatedAt = DateTime.UtcNow
            };

            conversation.ParticipantIds.AddRange(request.ParticipantIds);

            await _context.Conversations.InsertOneAsync(conversation);
            return await ConvertToConversationResponseAsync(conversation, currentUserId);
        }

        public async Task<List<ConversationResponse>> GetUserConversationsAsync(string userId)
        {
            var conversations = await _context.Conversations
                .Find(c => c.ParticipantIds.Contains(userId))
                .SortByDescending(c => c.LastMessageAt)
                .ToListAsync();

            var responseList = new List<ConversationResponse>();
            foreach (var conversation in conversations)
            {
                var response = await ConvertToConversationResponseAsync(conversation, userId);
                responseList.Add(response);
            }

            return responseList;
        }

        private async Task<ConversationResponse> ConvertToConversationResponseAsync(Conversation conversation, string currentUserId)
        {
            // Get participant user details
            var participantUsers = await _context.Users
                .Find(u => conversation.ParticipantIds.Contains(u.Id))
                .ToListAsync();

            var participants = participantUsers.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl
            }).ToList();

            // Get last message if exists
            LastMessageResponse? lastMessage = null;
            if (conversation.LastMessageId != null)
            {
                var message = await _context.Messages
                    .Find(m => m.Id == conversation.LastMessageId)
                    .FirstOrDefaultAsync();

                if (message != null)
                {
                    var sender = await _context.Users
                        .Find(u => u.Id == message.SenderId)
                        .FirstOrDefaultAsync();

                    lastMessage = new LastMessageResponse
                    {
                        Id = message.Id,
                        Content = message.Content,
                        CreatedAt = message.CreatedAt,
                        Sender = new SenderInfoDto
                        {
                            Id = sender.Id,
                            Username = sender.Username,
                            AvatarUrl = sender.AvatarUrl
                        }
                    };
                }
            }

            return new ConversationResponse
            {
                Id = conversation.Id,
                IsDirect = conversation.IsDirect,
                LastMessageAt = conversation.LastMessageAt,
                LastMessageId = conversation.LastMessageId,
                Participants = participants,
                SeenBy = conversation.SeenBy,
                UnreadCounts = conversation.UnreadCounts,
                CreatedAt = conversation.CreatedAt,
                LastMessage = lastMessage
            };
        }

        private async Task<Conversation?> FindDirectConversationAsync(string userId1, string userId2)
        {
            var filter = Builders<Conversation>.Filter.And(
                Builders<Conversation>.Filter.Eq(c => c.IsDirect, true),
                Builders<Conversation>.Filter.All(c => c.ParticipantIds, new[] { userId1, userId2 })
            );

            return await _context.Conversations
                .Find(filter)
                .FirstOrDefaultAsync();
        }
    }
}