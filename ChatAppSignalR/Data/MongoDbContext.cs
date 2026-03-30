using ChatAppSignalR.Models;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ChatAppSignalR.Settings;




namespace ChatAppSignalR.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;

        public MongoDbContext(IOptions<MongoDbSettings> options)
        {
            _settings = options.Value;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<User> Users =>
            _database.GetCollection<User>(_settings.UsersCollectionName);

        public IMongoCollection<Friend> Friends =>
            _database.GetCollection<Friend>(_settings.FriendsCollectionName);

        public IMongoCollection<Conversation> Conversations =>
            _database.GetCollection<Conversation>(_settings.ConversationsCollectionName);

        public IMongoCollection<Message> Messages =>
            _database.GetCollection<Message>(_settings.MessagesCollectionName);
    }
}

        public IMongoCollection<RefreshToken> RefreshTokens =>
            _database.GetCollection<RefreshToken>(_settings.RefreshTokensCollectionName);
    }
}
