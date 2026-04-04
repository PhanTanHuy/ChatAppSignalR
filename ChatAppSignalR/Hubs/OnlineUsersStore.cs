public static class OnlineUsersStore
{
    public static Dictionary<string, List<string>> OnlindeUsers = new();

    public static void Add(string UserId, string ConnectionId)
    {
        lock(OnlindeUsers)
        {
            if (!OnlindeUsers.ContainsKey(UserId))
            {
                OnlindeUsers[UserId] = new List<string>();
            }
            OnlindeUsers[UserId].Add(ConnectionId);
        }
    }

    public static void Remove(string UserId, string ConnectionId)
    {
        lock(OnlindeUsers)
        {
            if (!OnlindeUsers.ContainsKey(UserId)) return;

            OnlindeUsers[UserId].Remove(ConnectionId);

            if (OnlindeUsers[UserId].Count == 0) OnlindeUsers.Remove(UserId);
        }
    }

    public static IEnumerable<string> GetOnlineUsers()
    {
        return OnlindeUsers.Keys.ToList();
    }
}