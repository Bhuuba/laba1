using System.Threading.Tasks;

namespace NewMyApp.Core.Services.Interfaces
{
    public interface ICertificateService
    {
        Task<byte[]> GenerateCertificateAsync(string userId);
        Task<bool> IsEligibleForCertificateAsync(string userId);
    }
} 