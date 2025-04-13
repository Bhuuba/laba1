using System;

namespace NewMyApp.Core.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string UserId { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsAdmin { get; set; }
        
        public virtual Group Group { get; set; }
        public virtual User User { get; set; }
    }
} 