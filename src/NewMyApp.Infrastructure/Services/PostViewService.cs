using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Core.Services;
using NewMyApp.Infrastructure.Data;

namespace NewMyApp.Infrastructure.Services;

public class PostViewService : IPostViewService
{
    private readonly ApplicationDbContext _context;

    public PostViewService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task TrackViewAsync(int postId, string userId)
    {
        var existingView = await _context.PostViews
            .FirstOrDefaultAsync(v => v.PostId == postId && v.UserId == userId);

        if (existingView == null)
        {
            var view = new PostView
            {
                PostId = postId,
                UserId = userId,
                ViewedAt = DateTime.UtcNow
            };
            _context.PostViews.Add(view);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetTotalViewsForPostAsync(int postId)
    {
        return await _context.PostViews
            .CountAsync(v => v.PostId == postId);
    }

    public async Task<IEnumerable<Post>> GetTopPostsAsync(int count = 10)
    {
        return await _context.Posts
            .Select(p => new
            {
                Post = p,
                ViewCount = p.Views.Count
            })
            .OrderByDescending(x => x.ViewCount)
            .Take(count)
            .Select(x => x.Post)
            .ToListAsync();
    }

    public async Task<Certificate?> GenerateCertificateForTopUserAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Posts)
            .ThenInclude(p => p.Likes)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        var totalLikes = user.Posts.Sum(p => p.Likes.Count);
        var totalPosts = user.Posts.Count;

        if (totalPosts == 0 || totalLikes == 0)
            return null;

        var achievement = totalLikes >= 100 ? "Diamond Content Creator" :
                         totalLikes >= 50 ? "Gold Content Creator" :
                         totalLikes >= 25 ? "Silver Content Creator" :
                         "Bronze Content Creator";

        var description = $"This user has created {totalPosts} posts that have received {totalLikes} likes from the community. " +
                         "Their contributions have helped make our platform a more engaging and vibrant space for everyone.";

        return new Certificate
        {
            UserId = userId,
            UserName = $"{user.FirstName} {user.LastName}",
            TotalLikes = totalLikes,
            TotalPosts = totalPosts,
            GeneratedAt = DateTime.UtcNow,
            Achievement = achievement,
            Description = description
        };
    }
} 