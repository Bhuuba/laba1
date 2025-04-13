using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;
using NewMyApp.Web.Models;
using System.Threading.Tasks;

namespace NewMyApp.Web.Controllers;

[Authorize]
public class GroupsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IWebHostEnvironment _environment;

    public GroupsController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var groups = await _context.Groups
            .Include(g => g.UserGroups)
            .Include(g => g.Creator)
            .ToListAsync();

        var currentUser = await _userManager.GetUserAsync(User);
        ViewBag.CurrentUserId = currentUser?.Id;

        return View(groups);
    }

    public async Task<IActionResult> MyGroups()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var groups = await _context.Groups
            .Include(g => g.UserGroups)
            .Include(g => g.Creator)
            .Where(g => g.UserGroups.Any(ug => ug.UserId == currentUser.Id))
            .ToListAsync();

        return View(groups);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGroupViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var group = new Group
        {
            Name = model.Name,
            Description = model.Description,
            CreatedAt = DateTime.UtcNow,
            CreatorId = user.Id
        };

        if (model.CoverImage != null)
        {
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "groups");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.CoverImage.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.CoverImage.CopyToAsync(stream);
            }

            group.CoverImage = $"/uploads/groups/{fileName}";
        }

        _context.Groups.Add(group);

        var userGroup = new UserGroup
        {
            UserId = user.Id,
            Group = group,
            Role = GroupRole.Admin,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = group.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var group = await _context.Groups
            .Include(g => g.Creator)
            .Include(g => g.UserGroups)
                .ThenInclude(ug => ug.User)
            .Include(g => g.Posts)
                .ThenInclude(p => p.User)
            .Include(g => g.Posts)
                .ThenInclude(p => p.Likes)
            .Include(g => g.Posts)
                .ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        ViewBag.CurrentUserId = currentUser?.Id;
        ViewBag.IsAdmin = group.UserGroups.Any(ug => ug.UserId == currentUser?.Id && ug.Role == GroupRole.Admin);
        ViewBag.IsMember = group.UserGroups.Any(ug => ug.UserId == currentUser?.Id);

        return View(group);
    }

    [HttpPost]
    public async Task<IActionResult> Join(int groupId)
    {
        var group = await _context.Groups
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            return NotFound("Група не знайдена");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("Будь ласка, увійдіть в систему");
        }

        if (group.UserGroups.Any(ug => ug.UserId == user.Id))
        {
            return BadRequest("Ви вже є учасником цієї групи");
        }

        var userGroup = new UserGroup
        {
            UserId = user.Id,
            GroupId = groupId,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Leave(int groupId)
    {
        var group = await _context.Groups
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.Id == groupId);

        if (group == null)
        {
            return NotFound("Група не знайдена");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized("Будь ласка, увійдіть в систему");
        }

        var userGroup = group.UserGroups.FirstOrDefault(ug => ug.UserId == user.Id);
        if (userGroup == null)
        {
            return BadRequest("Ви не є учасником цієї групи");
        }

        if (userGroup.Role == GroupRole.Admin && group.UserGroups.Count(ug => ug.Role == GroupRole.Admin) == 1)
        {
            return BadRequest("Ви не можете покинути групу, оскільки ви єдиний адміністратор");
        }

        _context.UserGroups.Remove(userGroup);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MyGroups));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var group = await _context.Groups
            .Include(g => g.UserGroups)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound("Група не знайдена");
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null || !group.UserGroups.Any(ug => ug.UserId == currentUser.Id && ug.Role == GroupRole.Admin))
        {
            return Forbid("Тільки адміністратор може видалити групу");
        }

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(MyGroups));
    }
}