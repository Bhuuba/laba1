using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using NewMyApp.Core.Models;
using NewMyApp.Core.Services;
using NewMyApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace NewMyApp.Web.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public ChatHub(
        IChatService chatService,
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        _chatService = chatService;
        _userManager = userManager;
        _context = context;
    }

    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task SendMessage(string groupId, string content)
    {
        if (Context.User == null) return;
        
        var user = await _userManager.GetUserAsync(Context.User);
        if (user == null) return;

        var message = new ChatMessage
        {
            GroupId = int.Parse(groupId),
            UserId = user.Id,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        var savedMessage = await _chatService.AddMessageAsync(message);

        await Clients.Group(groupId).SendAsync("ReceiveMessage", new
        {
            messageId = savedMessage.Id,
            content = savedMessage.Content,
            userId = user.Id,
            userName = $"{user.FirstName} {user.LastName}",
            userAvatar = user.ProfilePicture,
            createdAt = savedMessage.CreatedAt
        });
    }

    public async Task DeleteMessage(int messageId)
    {
        if (Context.User == null) return;
        
        var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        await _chatService.DeleteMessageAsync(messageId, userId);
        
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId);
            
        if (message != null)
        {
            await Clients.Group(message.GroupId.ToString())
                .SendAsync("MessageDeleted", messageId);
        }
    }
}