using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace NewMyApp.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Group_{groupId}");
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Group_{groupId}");
        }

        public async Task SendMessage(string user, string message, string groupId)
        {
            await Clients.Group($"Group_{groupId}").SendAsync("ReceiveMessage", user, message);
        }
        
        public async Task DeleteMessage(int messageId, string groupId)
        {
            await Clients.Group($"Group_{groupId}").SendAsync("MessageDeleted", messageId);
        }
    }
}