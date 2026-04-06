using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppSignalR.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
    {
        // Context.UserIdentifier sẽ lấy ID từ CustomUserIdProvider bạn đã đăng ký
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;

        OnlineUsersStore.Add(userId!, connectionId);

        await Clients.All.SendAsync("online-users", OnlineUsersStore.GetOnlineUsers());
        
        await base.OnConnectedAsync();
    }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            OnlineUsersStore.Remove(userId!, connectionId);

            await Clients.All.SendAsync("online-users", OnlineUsersStore.GetOnlineUsers());

            await base.OnDisconnectedAsync(exception);
        }
    }
}