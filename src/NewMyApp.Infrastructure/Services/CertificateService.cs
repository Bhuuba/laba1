using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Core.Services;
using NewMyApp.Infrastructure.Data;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using System.IO;

namespace NewMyApp.Infrastructure.Services;

public class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _context;
    private const int MINIMUM_LIKES_FOR_CERTIFICATE = 1;

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

    public async Task<byte[]> GenerateCertificateAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Posts)
                .ThenInclude(p => p.Likes)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return null;

        var totalLikes = user.Posts.Sum(p => p.Likes.Count);
        var totalPosts = user.Posts.Count;

        if (!await IsEligibleForCertificateAsync(userId))
            return null;

        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);

        // Set fonts
        var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // Add header
        document.Add(new Paragraph("Certificate of Achievement")
            .SetFont(boldFont)
            .SetFontSize(24)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(30));

        // Add user info
        document.Add(new Paragraph($"This certificate is awarded to:")
            .SetFont(font)
            .SetFontSize(14)
            .SetTextAlignment(TextAlignment.CENTER));

        document.Add(new Paragraph($"{user.FirstName} {user.LastName}")
            .SetFont(boldFont)
            .SetFontSize(18)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20));

        // Add achievement level
        var achievement = DetermineAchievement(totalLikes);
        document.Add(new Paragraph(achievement)
            .SetFont(boldFont)
            .SetFontSize(16)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20));

        // Add statistics
        document.Add(new Paragraph($"Total Posts: {totalPosts}")
            .SetFont(font)
            .SetFontSize(12)
            .SetTextAlignment(TextAlignment.CENTER));

        document.Add(new Paragraph($"Total Likes: {totalLikes}")
            .SetFont(font)
            .SetFontSize(12)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20));

        // Add description
        var description = GenerateDescription(totalLikes, totalPosts);
        document.Add(new Paragraph(description)
            .SetFont(font)
            .SetFontSize(12)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(30));

        // Add date
        document.Add(new Paragraph($"Generated on: {DateTime.Now:MMMM dd, yyyy}")
            .SetFont(font)
            .SetFontSize(10)
            .SetTextAlignment(TextAlignment.CENTER));

        document.Close();
        return stream.ToArray();
    }

    private string DetermineAchievement(int totalLikes)
    {
        return totalLikes >= 50 ? "Diamond Content Creator" :
               totalLikes >= 25 ? "Gold Content Creator" :
               totalLikes >= 10 ? "Silver Content Creator" :
               "Bronze Content Creator";
    }

    private string GenerateDescription(int totalLikes, int totalPosts)
    {
        return $"This certificate recognizes the valuable contributions made to our community. " +
               $"With {totalPosts} posts and {totalLikes} likes, this user has demonstrated " +
               "engagement and quality content creation.";
    }
} 