using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAppSignalR.Models
{
    public class Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("conversationId")]
        public string ConversationId { get; set; } = null!;

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("senderId")]
        public string SenderId { get; set; } = null!;

        [BsonElement("content")]
        public string? Content { get; set; }

        [BsonElement("imgUrl")]
        public string? ImgUrl { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}