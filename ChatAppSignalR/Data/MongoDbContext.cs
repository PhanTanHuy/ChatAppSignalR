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

        public IMongoCollection<RefreshToken> RefreshTokens =>
            _database.GetCollection<RefreshToken>(_settings.RefreshTokensCollectionName);
    }
}
