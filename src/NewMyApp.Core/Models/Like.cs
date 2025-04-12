using System;

namespace NewMyApp.Core.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int? PostId { get; set; }
        public int? CommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Навігаційні властивості
        public virtual User User { get; set; } = null!;
        public virtual Post? Post { get; set; }
        public virtual Comment? Comment { get; set; }
        
        public Like()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
} 