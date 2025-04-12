using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;

namespace NewMyApp.Web.Controllers
{
    [Authorize]
    public class FriendsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public FriendsController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Friends
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var friends = await _context.Friendships
                .Include(f => f.Sender)
                .Include(f => f.Receiver)
                .Where(f => (f.SenderId == currentUser.Id || f.ReceiverId == currentUser.Id) 
                           && f.Status == FriendshipStatus.Accepted)
                .ToListAsync();

            var friendUsers = friends.Select(f => f.SenderId == currentUser.Id ? f.Receiver : f.Sender)
                                   .ToList();

            return View(friendUsers);
        }

        // GET: Friends/Requests
        public async Task<IActionResult> Requests()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var requests = await _context.Friendships
                .Include(f => f.Sender)
                .Where(f => f.ReceiverId == currentUser.Id && f.Status == FriendshipStatus.Pending)
                .ToListAsync();

            return View(requests);
        }

        // POST: Friends/Add/userId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            if (currentUser.Id == userId)
                return BadRequest("Не можна додати себе в друзі");

            var existingFriendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.SenderId == currentUser.Id && f.ReceiverId == userId) ||
                    (f.SenderId == userId && f.ReceiverId == currentUser.Id));

            if (existingFriendship != null)
                return BadRequest("Запит вже існує");

            var friendship = new Friendship
            {
                SenderId = currentUser.Id,
                ReceiverId = userId,
                Status = FriendshipStatus.Pending
            };

            _context.Friendships.Add(friendship);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Friends/Accept/userId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.SenderId == userId && f.ReceiverId == currentUser.Id);

            if (friendship == null)
                return NotFound();

            friendship.Status = FriendshipStatus.Accepted;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Friends/Reject/userId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.SenderId == userId && f.ReceiverId == currentUser.Id);

            if (friendship == null)
                return NotFound();

            friendship.Status = FriendshipStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Friends/Remove/userId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => 
                    (f.SenderId == currentUser.Id && f.ReceiverId == userId) ||
                    (f.SenderId == userId && f.ReceiverId == currentUser.Id));

            if (friendship == null)
                return NotFound();

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
} 