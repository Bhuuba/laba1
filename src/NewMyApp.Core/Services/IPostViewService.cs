using System.Threading.Tasks;
using NewMyApp.Core.Models;

namespace NewMyApp.Core.Services;

public interface IPostViewService
{
    Task<int> GetTotalViewsForPostAsync(int postId);
    Task<Certificate?> GenerateCertificateForTopUserAsync(string userId);
    Task TrackViewAsync(int postId, string userId);
} 