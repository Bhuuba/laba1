using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewMyApp.Core.Models;
using NewMyApp.Core.Services;
using NewMyApp.Infrastructure.Data;

namespace NewMyApp.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public UserProfileService(UserManager<User> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<User?> GetUserProfileAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<int> GetUserTotalViewsAsync(string userId)
    {
        return await _context.PostViews
            .CountAsync(pv => pv.Post != null && pv.Post.UserId == userId);
    }

    public async Task UpdateUserProfileAsync(string userId, string firstName, string lastName, string? bio, bool isEmailVisible)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Bio = bio;
        user.IsEmailVisible = isEmailVisible;

        await _userManager.UpdateAsync(user);
    }

    public async Task UpdateAvatarAsync(string userId, string? avatarUrl)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        user.ProfilePicture = avatarUrl;
        await _userManager.UpdateAsync(user);
    }
} 