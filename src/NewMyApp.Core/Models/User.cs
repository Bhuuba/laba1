using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace NewMyApp.Core.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActive { get; set; }
        public bool IsEmailVisible { get; set; }
        
        // Навігаційні властивості
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new HashSet<Like>();
        public virtual ICollection<Friendship> SentFriendships { get; set; } = new HashSet<Friendship>();
        public virtual ICollection<Friendship> ReceivedFriendships { get; set; } = new HashSet<Friendship>();
        public virtual ICollection<UserGroup> UserGroups { get; set; } = new HashSet<UserGroup>();
        public virtual ICollection<Group> Groups { get; set; } = new HashSet<Group>();
        
        public User()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
} 