using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatAppSignalR.Models
{
    public class Friend
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("userId")]
        public string UserId { get; set; } = null!;

        [BsonElement("friendUserId")]
        public string FriendUserId { get; set; } = null!;
    }
}