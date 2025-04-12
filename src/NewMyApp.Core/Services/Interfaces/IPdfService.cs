using System.Threading.Tasks;

namespace NewMyApp.Core.Services.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePostPdfAsync(int postId);
    }
} 