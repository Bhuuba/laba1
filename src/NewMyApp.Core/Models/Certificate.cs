using System;

namespace NewMyApp.Core.Models;

public class Certificate
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int TotalPosts { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string Achievement { get; set; } = string.Empty;
} 