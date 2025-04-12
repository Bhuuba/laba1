using System;
using System.Collections.Generic;

namespace NewMyApp.Core.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Навігаційні властивості
        public virtual ICollection<PostTag> PostTags { get; set; }
        
        public Tag()
        {
            PostTags = new HashSet<PostTag>();
            CreatedAt = DateTime.UtcNow;
        }
    }
} 