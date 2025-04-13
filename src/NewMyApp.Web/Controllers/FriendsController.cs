using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }
            
            // Get all accepted friendships where the current user is either sender or receiver
            var friendships = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .Where(fr => (fr.SenderId == currentUser.Id || fr.ReceiverId == currentUser.Id) && fr.IsAccepted)
                .ToListAsync();

            // Extract the friend users (excluding the current user)
            var friends = friendships
                .Select(fr => fr.SenderId == currentUser.Id ? fr.Receiver : fr.Sender)
                .ToList();

            return View(friends);
        }

        public async Task<IActionResult> Requests()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }
            
            var receivedRequests = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == currentUser.Id && !fr.IsAccepted)
                .ToListAsync() ?? new List<FriendRequest>();

            var sentRequests = await _context.FriendRequests
                .Include(fr => fr.Receiver)
                .Where(fr => fr.SenderId == currentUser.Id && !fr.IsAccepted)
                .ToListAsync() ?? new List<FriendRequest>();

            ViewBag.ReceivedRequests = receivedRequests;
            ViewBag.SentRequests = sentRequests;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendRequest(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }
            
            // Перевірка чи запит вже існує
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    (fr.SenderId == currentUser.Id && fr.ReceiverId == userId) ||
                    (fr.SenderId == userId && fr.ReceiverId == currentUser.Id));

            if (existingRequest != null)
            {
                return Json(new { success = false, message = "Запит вже надіслано" });
            }

            var friendRequest = new FriendRequest
            {
                SenderId = currentUser.Id,
                ReceiverId = userId,
                CreatedAt = DateTime.UtcNow,
                IsAccepted = false
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Запит надіслано" });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var request = await _context.FriendRequests.FindAsync(requestId);
            if (request == null || request.ReceiverId != currentUser.Id)
            {
                return NotFound();
            }

            request.IsAccepted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeclineRequest(int requestId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var request = await _context.FriendRequests.FindAsync(requestId);
            if (request == null || request.ReceiverId != currentUser.Id)
            {
                return NotFound();
            }

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            var friendship = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    ((fr.SenderId == currentUser.Id && fr.ReceiverId == userId) ||
                     (fr.SenderId == userId && fr.ReceiverId == currentUser.Id)) &&
                    fr.IsAccepted);

            if (friendship != null)
            {
                _context.FriendRequests.Remove(friendship);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 