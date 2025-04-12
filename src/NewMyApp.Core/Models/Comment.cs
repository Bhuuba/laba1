using System;
using System.Collections.Generic;

namespace NewMyApp.Core.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Зовнішні ключі
        public string UserId { get; set; } = string.Empty;
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
        
        // Навігаційні властивості
        public virtual User User { get; set; } = null!;
        public virtual Post Post { get; set; } = null!;
        public virtual Comment? ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new HashSet<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new HashSet<Like>();
        
        public Comment()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
} 