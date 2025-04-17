using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Core.Services;
using NewMyApp.Infrastructure.Data;

namespace NewMyApp.Infrastructure.Services.Chat
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveMessageAsync(int groupId, string userId, string content)
        {
            var message = new ChatMessage
            {
                GroupId = groupId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ChatMessage>> GetGroupMessagesAsync(int groupId)
        {
            return await _context.ChatMessages
                .Include(m => m.User)
                .Where(m => m.GroupId == groupId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> AddMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task DeleteMessageAsync(int messageId, string userId)
        {
            var message = await _context.ChatMessages.FindAsync(messageId);
            if (message != null && message.UserId == userId)
            {
                message.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}