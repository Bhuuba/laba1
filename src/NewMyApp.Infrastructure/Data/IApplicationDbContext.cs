using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;

namespace NewMyApp.Infrastructure.Data;

public interface IApplicationDbContext
{
    DbSet<Post> Posts { get; set; }
    DbSet<Comment> Comments { get; set; }
    DbSet<Group> Groups { get; set; }
    DbSet<Friendship> Friendships { get; set; }
    DbSet<Like> Likes { get; set; }
    DbSet<Tag> Tags { get; set; }
    DbSet<PostTag> PostTags { get; set; }
    DbSet<UserGroup> UserGroups { get; set; }
    DbSet<PostView> PostViews { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
} 