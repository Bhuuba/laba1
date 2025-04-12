using System;
using System.Collections.Generic;

namespace NewMyApp.Core.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Зовнішні ключі
        public string CreatorId { get; set; } = string.Empty;
        
        // Навігаційні властивості
        public virtual User Creator { get; set; } = null!;
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<UserGroup> UserGroups { get; set; }
        
        public Group()
        {
            Posts = new HashSet<Post>();
            UserGroups = new HashSet<UserGroup>();
            CreatedAt = DateTime.UtcNow;
        }
    }
} 