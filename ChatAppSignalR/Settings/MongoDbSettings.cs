namespace ChatAppSignalR.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string UsersCollectionName { get; set; } = "Users";
        public string RefreshTokensCollectionName { get; set; } = "RefreshTokens";
        public string FriendsCollectionName { get; set; } = "Friends";
        public string ConversationsCollectionName { get; set; } = "Conversations";
        public string MessagesCollectionName { get; set; } = "Messages";
    }
}
