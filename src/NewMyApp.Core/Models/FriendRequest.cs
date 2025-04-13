using System;

namespace NewMyApp.Core.Models
{
    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }

    public class FriendRequest
    {
        public int Id { get; set; }
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public DateTime CreatedAt { get; set; }
        public FriendRequestStatus Status { get; set; }
        
        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }
    }
} 