using System;

namespace NewMyApp.Core.Models
{
    public class Friendship
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public FriendshipStatus Status { get; set; }
        
        // Навігаційні властивості
        public virtual User Sender { get; set; } = null!;
        public virtual User Receiver { get; set; } = null!;
        
        public Friendship()
        {
            CreatedAt = DateTime.UtcNow;
            Status = FriendshipStatus.Pending;
        }
    }
    
    public enum FriendshipStatus
    {
        Pending,
        Accepted,
        Rejected,
        Blocked
    }
} 