using System.Threading.Tasks;
using NewMyApp.Core.Models;

namespace NewMyApp.Core.Services;

public interface IUserProfileService
{
    Task<User?> GetUserProfileAsync(string userId);
    Task<int> GetUserTotalViewsAsync(string userId);
    Task UpdateUserProfileAsync(string userId, string firstName, string lastName, string? bio, bool isEmailVisible);
    Task UpdateAvatarAsync(string userId, string? avatarUrl);
} 