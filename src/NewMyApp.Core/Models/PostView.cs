using System;

namespace NewMyApp.Core.Models;

public class PostView
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime ViewedAt { get; set; }

    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;

    public PostView()
    {
        ViewedAt = DateTime.UtcNow;
    }
} 