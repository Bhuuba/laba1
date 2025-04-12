using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;
using NewMyApp.Web.Models;

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
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return View(groups);
    }

    public async Task<IActionResult> MyGroups()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var groups = await _context.Groups
            .Include(g => g.UserGroups)
            .Where(g => g.UserGroups.Any(ug => ug.UserId == user.Id))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return View("Index", groups);
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
            .Include(g => g.UserGroups)
            .ThenInclude(ug => ug.User)
            .Include(g => g.Posts)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null)
        {
            return NotFound();
        }

        return View(group);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int id)
    {
        var group = await _context.Groups.FindAsync(id);
        if (group == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var existingMembership = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.GroupId == id && ug.UserId == user.Id);

        if (existingMembership != null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var userGroup = new UserGroup
        {
            UserId = user.Id,
            GroupId = id,
            Role = GroupRole.Member,
            JoinedAt = DateTime.UtcNow
        };

        _context.UserGroups.Add(userGroup);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var membership = await _context.UserGroups
            .FirstOrDefaultAsync(ug => ug.GroupId == id && ug.UserId == user.Id);

        if (membership == null)
        {
            return NotFound();
        }

        if (membership.Role == GroupRole.Admin)
        {
            var otherAdmins = await _context.UserGroups
                .CountAsync(ug => ug.GroupId == id && ug.Role == GroupRole.Admin && ug.UserId != user.Id);

            if (otherAdmins == 0)
            {
                ModelState.AddModelError(string.Empty, "Ви не можете покинути групу, оскільки ви єдиний адміністратор");
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        _context.UserGroups.Remove(membership);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
} 