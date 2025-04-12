using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Core.Services;
using NewMyApp.Infrastructure.Data;

namespace NewMyApp.Infrastructure.Services;

public class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _context;
    private const int MINIMUM_LIKES_FOR_CERTIFICATE = 10;

    public CertificateService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEligibleForCertificateAsync(string userId)
    {
        var totalLikes = await _context.Posts
            .Where(p => p.UserId == userId)
            .SelectMany(p => p.Likes)
            .CountAsync();

        return totalLikes >= MINIMUM_LIKES_FOR_CERTIFICATE;
    }

    public async Task<Certificate> GenerateCertificateAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new ArgumentException("User not found", nameof(userId));

        var posts = await _context.Posts
            .Where(p => p.UserId == userId)
            .Include(p => p.Likes)
            .ToListAsync();

        var totalLikes = posts.Sum(p => p.Likes.Count);
        var totalPosts = posts.Count;

        var achievement = DetermineAchievement(totalLikes);
        var description = GenerateDescription(totalLikes, totalPosts);

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

    private string DetermineAchievement(int totalLikes)
    {
        return totalLikes switch
        {
            >= 100 => "Diamond Content Creator",
            >= 50 => "Gold Content Creator",
            >= 25 => "Silver Content Creator",
            _ => "Bronze Content Creator"
        };
    }

    private string GenerateDescription(int totalLikes, int totalPosts)
    {
        return $"This user has created {totalPosts} posts that have received {totalLikes} likes from the community. " +
               "Their contributions have helped make our platform a more engaging and vibrant space for everyone.";
    }
} 