using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewMyApp.Core.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? CoverImage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        
        // Зовнішні ключі
        public string CreatorId { get; set; } = string.Empty;
        
        // Навігаційні властивості
        public virtual User Creator { get; set; } = null!;
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
        public virtual ICollection<UserGroup> UserGroups { get; set; } = new HashSet<UserGroup>();
        public virtual ICollection<GroupMember> Members { get; set; } = new HashSet<GroupMember>();
        
        public Group()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
} 