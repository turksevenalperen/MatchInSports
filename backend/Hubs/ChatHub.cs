using Microsoft.AspNetCore.SignalR;

namespace MatchISportsAPI.Services;

public class ChatHub : Hub
{
    public async Task SendMessage(string receiverId, string message)
    {
        var senderId = Context.UserIdentifier;
        
        await Clients.User(receiverId).SendAsync("ReceiveMessage", new
        {
            senderId,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task LeaveRoom(string roomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
    }

    public async Task SendGroupMessage(string roomId, string message)
    {
        await Clients.Group(roomId).SendAsync("ReceiveGroupMessage", new
        {
            senderId = Context.UserIdentifier,
            message,
            timestamp = DateTime.UtcNow
        });
    }
}
