using System.Collections.Generic;
using NewMyApp.Core.Models;

namespace NewMyApp.Web.Models;

public class SearchResultViewModel
{
    public string SearchTerm { get; set; } = string.Empty;
    public List<User> Users { get; set; } = new();
    public List<Post> Posts { get; set; } = new();
    public List<Group> Groups { get; set; } = new();
} 