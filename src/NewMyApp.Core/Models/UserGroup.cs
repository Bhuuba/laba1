using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewMyApp.Core.Models
{
    public class UserGroup
    {
        [Key]
        [Column(Order = 0)]
        public string UserId { get; set; } = string.Empty;
        
        [Key]
        [Column(Order = 1)]
        public int GroupId { get; set; }
        
        public DateTime JoinedAt { get; set; }
        public GroupRole Role { get; set; }
        
        // Навігаційні властивості
        public virtual User User { get; set; } = null!;
        public virtual Group Group { get; set; } = null!;
        
        public UserGroup()
        {
            JoinedAt = DateTime.UtcNow;
            Role = GroupRole.Member;
        }
    }
    
    public enum GroupRole
    {
        Member,
        Moderator,
        Admin
    }
} 