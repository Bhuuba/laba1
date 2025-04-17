using System;

namespace NewMyApp.Core.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
    
    // Внешние ключи
    public string UserId { get; set; } = string.Empty;
    public int GroupId { get; set; }
    
    // Навигационные свойства
    public virtual User User { get; set; } = null!;
    public virtual Group Group { get; set; } = null!;
    
    public ChatMessage()
    {
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }
}