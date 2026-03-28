using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppSignalR.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
    }
}