using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;
using NewMyApp.Web.Models;
using NewMyApp.Core.Services;

namespace NewMyApp.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProfileController> _logger;
    private readonly ICertificateService _certificateService;

    public ProfileController(
        UserManager<User> userManager,
        ApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<ProfileController> logger,
        ICertificateService certificateService)
    {
        _userManager = userManager;
        _context = context;
        _environment = environment;
        _logger = logger;
        _certificateService = certificateService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var viewModel = new ProfileViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            ProfilePicture = user.ProfilePicture,
            Bio = user.Bio,
            DateOfBirth = user.DateOfBirth,
            TotalPosts = await _context.Posts.CountAsync(p => p.UserId == user.Id),
            TotalFriends = await _context.Friendships
                .CountAsync(f => (f.SenderId == user.Id || f.ReceiverId == user.Id) && f.Status == FriendshipStatus.Accepted),
            TotalGroups = await _context.UserGroups.CountAsync(ug => ug.UserId == user.Id)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Bio = model.Bio;
        user.DateOfBirth = model.DateOfBirth;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["StatusMessage"] = "Профіль успішно оновлено";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdatePhoto(IFormFile photoFile)
    {
        if (photoFile == null || photoFile.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Будь ласка, виберіть файл");
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Validate file type
        var allowedTypes = new[] { "image/jpeg", "image/png" };
        if (!allowedTypes.Contains(photoFile.ContentType.ToLower()))
        {
            ModelState.AddModelError(string.Empty, "Дозволені тільки файли формату JPG та PNG");
            return RedirectToAction(nameof(Index));
        }

        // Validate file size (5MB max)
        if (photoFile.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(string.Empty, "Розмір файлу не може перевищувати 5MB");
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsDir);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photoFile.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(stream);
            }

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var oldFilePath = Path.Combine(_environment.WebRootPath, user.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Update user profile picture path
            user.ProfilePicture = $"/uploads/profiles/{fileName}";
            await _userManager.UpdateAsync(user);

            TempData["StatusMessage"] = "Фото профілю успішно оновлено";
        }
        catch (Exception ex)
        {
            // Log the error
            _logger.LogError(ex, "Error uploading profile photo");
            ModelState.AddModelError(string.Empty, "Помилка при завантаженні фото. Спробуйте ще раз.");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Certificate()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        if (!await _certificateService.IsEligibleForCertificateAsync(user.Id))
        {
            TempData["Error"] = "You need at least 10 likes on your posts to be eligible for a certificate.";
            return RedirectToAction(nameof(Index));
        }

        var certificate = await _certificateService.GenerateCertificateAsync(user.Id);
        return View(certificate);
    }
} 