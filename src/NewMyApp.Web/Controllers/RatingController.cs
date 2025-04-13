using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Web.Models;
using NewMyApp.Infrastructure.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using System.Threading.Tasks;

namespace NewMyApp.Web.Controllers
{
    [Authorize]
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public RatingController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .ToListAsync();

            var topUsers = users
                .Select(u => new TopUserViewModel
                {
                    User = u,
                    TotalLikes = u.Posts.Sum(p => p.Likes.Count),
                    Rank = 0
                })
                .OrderByDescending(u => u.TotalLikes)
                .Take(50)
                .ToList();

            for (int i = 0; i < topUsers.Count; i++)
            {
                topUsers[i].Rank = i + 1;
            }

            return View(topUsers);
        }

        public async Task<IActionResult> DownloadTopUserPdf()
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var currentUser = await _userManager.GetUserAsync(User);

            var userWithPosts = await _context.Users
                .Where(u => u.Id == currentUser.Id)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .FirstOrDefaultAsync();

            var totalLikes = userWithPosts.Posts.Sum(p => p.Likes.Count);

            var allUsers = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .ToListAsync();

            var rankedUsers = allUsers
                .Select(u => new
                {
                    User = u,
                    TotalLikes = u.Posts.Sum(p => p.Likes.Count)
                })
                .OrderByDescending(u => u.TotalLikes)
                .ToList();

            var rank = rankedUsers.FindIndex(u => u.User.Id == currentUser.Id) + 1;

            if (rank != 1)
            {
                return Forbid();
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(20));

                    page.Header().Text("Сертифікат переможця").SemiBold().FontSize(36).AlignCenter();
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().Text($"Цей сертифікат підтверджує, що {currentUser.FirstName} {currentUser.LastName}");
                        col.Item().Text($"здобув перше місце у рейтингу користувачів за кількістю лайків.");
                        col.Item().Text($"Загальна кількість лайків: {totalLikes}");
                        col.Item().Text($"Ранг: {rank}");
                    });
                    page.Footer().AlignCenter().Text(txt =>
                    {
                        txt.Span("NewMyApp © ").FontSize(12);
                        txt.Span($"{DateTime.UtcNow.Year}").FontSize(12);
                    });
                });
            });

            var stream = new MemoryStream();
            pdf.GeneratePdf(stream);
            stream.Position = 0;

            return File(stream, "application/pdf", "TopUserCertificate.pdf");
        }

        [HttpPost]
        public async Task<IActionResult> LikePost(int id)
        {
            var post = await _context.Posts.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (post.Likes.Any(l => l.UserId == currentUser.Id))
            {
                return NoContent(); // Если уже лайкнуто, ничего не возвращаем
            }

            post.Likes.Add(new Like { UserId = currentUser.Id });
            await _context.SaveChangesAsync();

            return NoContent(); // Убираем вывод JSON, возвращаем пустой ответ
        }
    }
}
