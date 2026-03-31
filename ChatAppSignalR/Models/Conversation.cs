using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAppSignalR.Models
{
    public class Conversation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("participantIds")]
        public List<string> ParticipantIds { get; set; } = new();

        [BsonElement("isDirect")]
        public bool IsDirect { get; set; }

        [BsonElement("seenBy")]
        public List<string> SeenBy { get; set; } = new();

        [BsonElement("unreadCounts")]
        public Dictionary<string, int> UnreadCounts { get; set; } = new();

        [BsonElement("lastMessageId")]
        public string? LastMessageId { get; set; }

        [BsonElement("lastMessageAt")]
        public DateTime? LastMessageAt { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}