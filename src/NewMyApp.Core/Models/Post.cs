using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewMyApp.Core.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Зовнішні ключі
        public string UserId { get; set; } = string.Empty;
        public int? GroupId { get; set; }
        
        // Навігаційні властивості
        public virtual User User { get; set; } = null!;
        public virtual Group? Group { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new HashSet<Like>();
        public virtual ICollection<PostTag> PostTags { get; set; } = new HashSet<PostTag>();
        public virtual ICollection<PostView> Views { get; set; } = new HashSet<PostView>();

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        
        public Post()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
} 