using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Core.Services;
using NewMyApp.Infrastructure.Data;
using System.Threading.Tasks;

namespace NewMyApp.Web.Controllers;

[Route("api/chat")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{groupId}/messages")]
    public async Task<IActionResult> GetMessages(int groupId)
    {
        var messages = await _context.ChatMessages
            .Include(m => m.User)
            .Where(m => m.GroupId == groupId && !m.IsDeleted)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new
            {
                id = m.Id,
                content = m.Content,
                userId = m.UserId,
                userName = $"{m.User.FirstName} {m.User.LastName}",
                userAvatar = m.User.ProfilePicture ?? "/images/default-avatar.png",
                createdAt = m.CreatedAt.ToString("dd.MM.yyyy HH:mm")
            })
            .ToListAsync();

        return Ok(messages);
    }
}