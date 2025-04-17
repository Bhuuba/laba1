using System.Collections.Generic;
using System.Threading.Tasks;
using NewMyApp.Core.Models;

namespace NewMyApp.Core.Services
{
    public interface IChatService
    {
        Task SaveMessageAsync(int groupId, string userId, string content);
        Task<List<ChatMessage>> GetGroupMessagesAsync(int groupId);
        Task<ChatMessage> AddMessageAsync(ChatMessage message);
        Task DeleteMessageAsync(int messageId, string userId);
    }
}