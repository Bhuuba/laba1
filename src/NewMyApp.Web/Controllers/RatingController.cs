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
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Elements;

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
            if (currentUser == null) return NotFound();

            var userWithPosts = await _context.Users
                .Where(u => u.Id == currentUser.Id)
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .FirstOrDefaultAsync();

            if (userWithPosts == null) return NotFound();

            var totalLikes = userWithPosts.Posts.Sum(p => p.Likes.Count);
            var allUsers = await _context.Users
                .Include(u => u.Posts)
                    .ThenInclude(p => p.Likes)
                .ToListAsync();

            var rankedUsers = allUsers
                .Select(u => new { User = u, TotalLikes = u.Posts.Sum(p => p.Likes.Count) })
                .OrderByDescending(u => u.TotalLikes)
                .ToList();

            var rank = rankedUsers.FindIndex(u => u.User.Id == currentUser.Id) + 1;

            if (rank != 1) return Forbid();

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(20));
                    page.Background(Colors.White);

                    // Заголовок
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Background(Colors.Blue.Lighten3).Padding(20).Column(col =>
                        {
                            col.Item().AlignCenter().Text("Сертифікат Досягнень")
                                .SemiBold()
                                .FontSize(36)
                                .FontColor(Colors.Grey.Darken4);
                        });
                    });

                    // Основной контент
                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().AlignCenter().Text("🏆").FontSize(50);
                        
                        col.Item().AlignCenter().PaddingVertical(20).Text("Цей сертифікат засвідчує, що")
                            .FontSize(16)
                            .FontColor(Colors.Grey.Medium);

                        col.Item().AlignCenter().Text($"{currentUser.FirstName} {currentUser.LastName}")
                            .SemiBold()
                            .FontSize(28)
                            .FontColor(Colors.Blue.Darken2);

                        col.Item().AlignCenter().PaddingTop(10)
                            .Text("здобув перше місце у рейтингу користувачів")
                            .FontSize(18);

                        col.Item().PaddingVertical(20).AlignCenter().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(statCol =>
                        {
                            statCol.Item().AlignCenter().Text("Загальна кількість лайків")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Medium);
                            statCol.Item().AlignCenter().Text(totalLikes.ToString())
                                .SemiBold()
                                .FontSize(24)
                                .FontColor(Colors.Blue.Darken2);
                        });

                        col.Item().PaddingTop(20).AlignCenter()
                            .Text($"Дата видачі: {DateTime.Now:dd.MM.yyyy}")
                            .FontSize(14)
                            .FontColor(Colors.Grey.Medium);
                    });

                    // Футер
                    page.Footer().AlignCenter().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(10).Text(text =>
                        {
                            text.Span("NewMyApp © ").FontSize(12).FontColor(Colors.Grey.Medium);
                            text.Span(DateTime.Now.Year.ToString()).FontSize(12).FontColor(Colors.Grey.Medium);
                        });
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
            if (post == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (post.Likes.Any(l => l.UserId == currentUser.Id)) return NoContent();

            post.Likes.Add(new Like { UserId = currentUser.Id });
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
