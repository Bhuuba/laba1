using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using NewMyApp.Web.Models;

namespace NewMyApp.Web.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostsController(ApplicationDbContext context, UserManager<User> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Group)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Group)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                    .ThenInclude(l => l.User)
                .Include(p => p.PostTags)
                    .ThenInclude(pt => pt.Tag)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Posts/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Groups = await _context.Groups
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
            return View();
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || post.UserId != user.Id)
            {
                return Forbid();
            }

            return View(post);
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreatePostViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            try
            {
                var post = new Post
                {
                    Content = model.Content,
                    UserId = user.Id,
                    GroupId = model.GroupId,
                    CreatedAt = DateTime.UtcNow
                };

                if (model.ImageFile != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posts");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    post.ImageUrl = $"/uploads/posts/{uniqueFileName}";
                }

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Помилка при створенні поста. Будь ласка, спробуйте ще раз.");
                return View(model);
            }
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (post.UserId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Posts/Like/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return Json(new { success = false, message = "Пост не найден" });
            }

            var userId = _userManager.GetUserId(User);
            var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);

            if (existingLike != null)
            {
                post.Likes.Remove(existingLike);
            }
            else
            {
                post.Likes.Add(new Like { PostId = post.Id, UserId = userId });
            }

            await _context.SaveChangesAsync();

            return Json(new {
                success = true,
                isLiked = existingLike == null,
                likesCount = post.Likes.Count
            });
        }

        // POST: Posts/CreateComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment([FromForm] CreateCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Details), new { id = model.PostId });
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var comment = new Comment
            {
                Content = model.Content,
                PostId = model.PostId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ParentCommentId = model.ParentCommentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = model.PostId });
        }

        // GET: Posts/MyPosts
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Group)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View("Index", posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LikeComment(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Unauthorized();
            }

            var comment = await _context.Comments
                .Include(c => c.Likes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound();
            }

            var existingLike = comment.Likes.FirstOrDefault(l => l.UserId == userId);
            if (existingLike != null)
            {
                comment.Likes.Remove(existingLike);
            }
            else
            {
                comment.Likes.Add(new Like { UserId = userId });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = comment.PostId });
        }

        // POST: Posts/DeleteComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int commentId, int postId)
        {
            var comment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
            {
                TempData["Error"] = "Коментар не знайдено.";
                return RedirectToAction(nameof(Details), new { id = postId });
            }

            var userId = _userManager.GetUserId(User);
            // Дозволяємо видалення, якщо користувач є автором коментаря або автором поста
            if (userId != comment.UserId && userId != comment.Post.UserId)
            {
                TempData["Error"] = "У вас немає прав на видалення цього коментаря.";
                return RedirectToAction(nameof(Details), new { id = postId });
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Коментар успішно видалено.";
            return RedirectToAction(nameof(Details), new { id = postId });
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] EditPostViewModel model)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null || post.UserId != user.Id)
            {
                return Forbid();
            }

            try
            {
                post.Content = model.Content;

                if (model.ImageFile != null)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posts");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(post.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ImageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    post.ImageUrl = $"/uploads/posts/{uniqueFileName}";
                }

                _context.Update(post);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Помилка при редагуванні поста. Будь ласка, спробуйте ще раз.");
                return View(post);
            }
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}