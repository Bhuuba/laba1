using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Services.Interfaces;
using NewMyApp.Core.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;
using NewMyApp.Core.Models;

namespace NewMyApp.Core.Services
{
    public class PdfService : IPdfService
    {
        private readonly IPostRepository _postRepository;

        public PdfService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public async Task<byte[]> GeneratePostPdfAsync(int postId)
        {
            var post = await _postRepository.GetPostWithDetailsAsync(postId);
            if (post == null)
                throw new ArgumentException("Post not found", nameof(postId));

            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Set up fonts
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            // Add post title
            document.Add(new Paragraph(post.Content)
                .SetFontSize(16)
                .SetFont(boldFont)
                .SetTextAlignment(TextAlignment.CENTER));

            // Add post details
            document.Add(new Paragraph($"Author: {post.User?.UserName ?? "Unknown"}")
                .SetFontSize(12)
                .SetFont(font));
            document.Add(new Paragraph($"Created: {post.CreatedAt}")
                .SetFontSize(12)
                .SetFont(font));

            // Add comments section
            document.Add(new Paragraph("Comments:")
                .SetFontSize(14)
                .SetFont(boldFont)
                .SetMarginTop(20));

            if (post.Comments != null)
            {
                foreach (var comment in post.Comments)
                {
                    var commentText = new Text($"{comment.User?.UserName ?? "Unknown"}: ")
                        .SetFont(boldFont);
                    var contentText = new Text(comment.Content)
                        .SetFont(font);

                    document.Add(new Paragraph()
                        .Add(commentText)
                        .Add(contentText)
                        .SetFontSize(12)
                        .SetMarginLeft(20));
                }
            }

            document.Close();
            return memoryStream.ToArray();
        }
    }
} 