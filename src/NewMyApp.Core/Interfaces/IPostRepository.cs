using System.Threading.Tasks;
using NewMyApp.Core.Models;

namespace NewMyApp.Core.Interfaces
{
    public interface IPostRepository
    {
        Task<Post> GetPostWithDetailsAsync(int postId);
    }
} 