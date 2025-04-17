using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;

namespace NewMyApp.Web.Controllers
{
    [Route("api/chat")]
    [ApiController]
    [Authorize]
    public class ChatApiController : ControllerBase
    {
        private readonly IApplicationDbContext _context;

        public ChatApiController(IApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{groupId}/messages")]
        public async Task<IActionResult> GetMessages(int groupId)
        {
            var messages = await _context.ChatMessages
                .Include(m => m.User)
                .Where(m => m.GroupId == groupId && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Take(50)
                .Select(m => new
                {
                    id = m.Id,
                    content = m.Content,
                    userId = m.UserId,
                    userName = m.User.FirstName + " " + m.User.LastName,
                    userAvatar = m.User.ProfilePicture ?? "/images/default-avatar.png",
                    createdAt = m.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                })
                .ToListAsync();

            return Ok(messages.OrderBy(m => m.createdAt));
        }
    }
}