using System;

namespace NewMyApp.Core.Models
{
    public class FriendRequest
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAccepted { get; set; }
        
        public virtual User Sender { get; set; }
        public virtual User Receiver { get; set; }
    }
} 