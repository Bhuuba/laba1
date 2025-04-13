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
                return NotFound();
            }

            var friends = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .Where(fr => (fr.SenderId == currentUser.Id || fr.ReceiverId == currentUser.Id) && fr.Status == FriendRequestStatus.Accepted)
                .ToListAsync();

            var friendUsers = friends
                .Select(fr => fr.SenderId == currentUser.Id ? fr.Receiver : fr.Sender)
                .ToList();

            return View(friendUsers);
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
                .Where(fr => fr.ReceiverId == currentUser.Id && fr.Status == FriendRequestStatus.Pending)
                .ToListAsync();

            var sentRequests = await _context.FriendRequests
                .Include(fr => fr.Receiver)
                .Where(fr => fr.SenderId == currentUser.Id && fr.Status == FriendRequestStatus.Pending)
                .ToListAsync();

            ViewBag.ReceivedRequests = receivedRequests;
            ViewBag.SentRequests = sentRequests;

            return View();
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return View(new List<User>());
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            // Отримуємо список ID всіх друзів та користувачів з активними запитами
            var friendsAndRequestsIds = await Task.FromResult(
                _context.FriendRequests
                    .Where(fr => 
                        (fr.SenderId == currentUser.Id || fr.ReceiverId == currentUser.Id) &&
                        (fr.Status == FriendRequestStatus.Accepted || fr.Status == FriendRequestStatus.Pending))
                    .AsEnumerable() // Переключаемся на клиентскую обработку
                    .SelectMany(fr => new[] { fr.SenderId, fr.ReceiverId })
                    .Distinct()
                    .ToList()); // Убираем асинхронность, так как ToList уже возвращает синхронный результат

            // Шукаємо користувачів, які відповідають пошуковому запиту
            var users = await _userManager.Users
                .Where(u => u.Id != currentUser.Id && 
                           !friendsAndRequestsIds.Contains(u.Id) &&
                           (u.UserName.Contains(searchTerm) || 
                            u.Email.Contains(searchTerm)))
                .Take(10)
                .ToListAsync();

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendFriendRequest(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId))
            {
                return BadRequest();
            }

            var sender = await _userManager.GetUserAsync(User);
            if (sender == null)
            {
                return Challenge();
            }

            var receiver = await _userManager.FindByIdAsync(receiverId);
            if (receiver == null)
            {
                return NotFound();
            }

            // Перевірка чи запит вже існує
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    (fr.SenderId == sender.Id && fr.ReceiverId == receiver.Id) ||
                    (fr.SenderId == receiver.Id && fr.ReceiverId == sender.Id));

            if (existingRequest != null)
            {
                TempData["Error"] = "Запит в друзі вже надіслано або ви вже є друзями";
                return RedirectToAction(nameof(Search));
            }

            var friendRequest = new FriendRequest
            {
                SenderId = sender.Id,
                ReceiverId = receiver.Id,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Запит в друзі успішно надіслано";
            return RedirectToAction(nameof(Search));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (request == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || request.ReceiverId != currentUser.Id)
            {
                return Unauthorized();
            }

            request.Status = FriendRequestStatus.Accepted;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Requests));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineRequest(int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId);

            if (request == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || (request.ReceiverId != currentUser.Id && request.SenderId != currentUser.Id))
            {
                return Unauthorized();
            }

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Requests));
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
                    fr.Status == FriendRequestStatus.Accepted);

            if (friendship != null)
            {
                _context.FriendRequests.Remove(friendship);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            // Знаходимо всі дружні зв'язки між користувачами
            var friendships = await _context.FriendRequests
                .Where(fr => 
                    (fr.SenderId == currentUser.Id && fr.ReceiverId == userId) ||
                    (fr.SenderId == userId && fr.ReceiverId == currentUser.Id))
                .Where(fr => fr.Status == FriendRequestStatus.Accepted)
                .ToListAsync();

            if (!friendships.Any())
            {
                return NotFound();
            }

            _context.FriendRequests.RemoveRange(friendships);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}