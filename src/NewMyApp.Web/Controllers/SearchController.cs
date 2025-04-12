using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;
using NewMyApp.Web.Models;

namespace NewMyApp.Web.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public SearchController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return View(new SearchResultViewModel());
        }

        var searchTerm = q.ToLower();

        var users = await _context.Users
            .Where(u => (u.FirstName != null && u.FirstName.ToLower().Contains(searchTerm)) ||
                       (u.LastName != null && u.LastName.ToLower().Contains(searchTerm)) ||
                       (u.Email != null && u.Email.ToLower().Contains(searchTerm)))
            .Take(10)
            .ToListAsync();

        var posts = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Where(p => p.Content != null && p.Content.ToLower().Contains(searchTerm))
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .ToListAsync();

        var groups = await _context.Groups
            .Include(g => g.UserGroups)
            .Where(g => (g.Name != null && g.Name.ToLower().Contains(searchTerm)) ||
                       (g.Description != null && g.Description.ToLower().Contains(searchTerm)))
            .Take(10)
            .ToListAsync();

        var viewModel = new SearchResultViewModel
        {
            SearchTerm = q,
            Users = users,
            Posts = posts,
            Groups = groups
        };

        return View(viewModel);
    }
} 