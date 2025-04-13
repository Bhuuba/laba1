using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;
using NewMyApp.Web.Hubs;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewMyApp.Web.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(
            ApplicationDbContext context, 
            UserManager<User> userManager,
            IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.UserGroups)
                    .ThenInclude(ug => ug.User)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
            {
                return NotFound("Група не знайдена");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || !group.UserGroups.Any(ug => ug.UserId == currentUser.Id))
            {
                return Forbid("Ви не є учасником цієї групи");
            }

            var messages = await _context.Messages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.User)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.Group = group;
            ViewBag.IsAdmin = group.UserGroups.Any(ug => ug.UserId == currentUser.Id && ug.Role == GroupRole.Admin);
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int groupId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return BadRequest("Повідомлення не може бути порожнім");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized("Будь ласка, увійдіть в систему");
            }

            // Перевіряємо, чи користувач є учасником групи
            var isMember = await _context.UserGroups
                .AnyAsync(ug => ug.UserId == currentUser.Id && ug.GroupId == groupId);

            if (!isMember)
            {
                return Forbid("Ви не є учасником цієї групи");
            }

            try
            {
                // Створюємо і зберігаємо повідомлення
                var message = new Message
                {
                    Content = content,
                    GroupId = groupId,
                    UserId = currentUser.Id,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Отримуємо повідомлення з бази даних з включеною інформацією про користувача
                var savedMessage = await _context.Messages
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == message.Id);

                if (savedMessage == null)
                {
                    return BadRequest("Не вдалося зберегти повідомлення");
                }

                // Формуємо HTML для нового повідомлення
                var isCurrentUser = savedMessage.UserId == currentUser.Id;
                bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin") || 
                               await _context.UserGroups.AnyAsync(ug => 
                                   ug.UserId == currentUser.Id && 
                                   ug.GroupId == groupId && 
                                   ug.Role == GroupRole.Admin);

                var htmlBuilder = new StringBuilder();
                htmlBuilder.Append($"<div class=\"message-item mb-3 {(isCurrentUser ? "text-end" : "text-start")}\" data-message-id=\"{savedMessage.Id}\" id=\"message-{savedMessage.Id}\">");
                htmlBuilder.Append($"<div class=\"d-inline-block message-bubble p-3 rounded-3 shadow-sm\" style=\"max-width: 80%; background-color: {(isCurrentUser ? "#dcf8c6" : "#ffffff")}; border-radius: 15px;\">");
                htmlBuilder.Append($"<div class=\"d-flex justify-content-between align-items-center mb-1\">");
                htmlBuilder.Append($"<strong class=\"text-primary\">{savedMessage.User.FirstName} {savedMessage.User.LastName}</strong>");
                htmlBuilder.Append($"<span class=\"message-time text-muted small\">{savedMessage.SentAt.ToString("dd.MM.yyyy HH:mm")}</span>");
                htmlBuilder.Append($"</div>");
                htmlBuilder.Append($"<p class=\"message-content mb-0\">{savedMessage.Content}</p>");
                htmlBuilder.Append($"</div>");
                
                if (isCurrentUser || isAdmin)
                {
                    htmlBuilder.Append($"<button type=\"button\" class=\"btn btn-sm btn-danger delete-message-btn mt-1\" data-message-id=\"{savedMessage.Id}\" data-bs-toggle=\"tooltip\" title=\"Видалити повідомлення\">");
                    htmlBuilder.Append($"<i class=\"bi bi-trash\"></i> Видалити</button>");
                }
                
                htmlBuilder.Append($"</div>");

                // Відправляємо повідомлення всім клієнтам через SignalR
                await _hubContext.Clients.Group($"Group_{groupId}").SendAsync("ReceiveMessage", htmlBuilder.ToString());

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                return StatusCode(500, "Виникла помилка при відправці повідомлення");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var message = await _context.Messages
                    .Include(m => m.User)
                    .FirstOrDefaultAsync(m => m.Id == messageId);

                if (message == null)
                {
                    return NotFound("Повідомлення не знайдено");
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    return Unauthorized("Будь ласка, увійдіть в систему");
                }

                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
                
                // Перевіряємо, чи користувач має право видалити це повідомлення
                if (message.UserId != currentUser.Id && !isAdmin)
                {
                    // Перевіряємо, чи користувач є адміністратором групи
                    var isGroupAdmin = await _context.UserGroups
                        .AnyAsync(ug => ug.UserId == currentUser.Id && ug.GroupId == message.GroupId && ug.Role == GroupRole.Admin);
                    
                    if (!isGroupAdmin)
                    {
                        return Forbid("Ви не маєте прав на видалення цього повідомлення");
                    }
                }

                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                // Сповіщаємо всіх клієнтів про видалення повідомлення
                await _hubContext.Clients.Group($"Group_{message.GroupId}").SendAsync("MessageDeleted", messageId);

                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting message: {ex.Message}");
                return StatusCode(500, "Виникла помилка при видаленні повідомлення");
            }
        }
    }
}