using System;

namespace NewMyApp.Core.Models;

public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsEmailVisible { get; set; }
    public string? Bio { get; set; }
    public string AvatarUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActive { get; set; }
    public int TotalPosts { get; set; }
    public int TotalFriends { get; set; }

    public User User { get; set; } = null!;

    public UserProfile()
    {
        CreatedAt = DateTime.UtcNow;
        IsEmailVisible = false;
    }
} 