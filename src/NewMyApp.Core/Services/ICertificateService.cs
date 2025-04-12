using System.Threading.Tasks;
using NewMyApp.Core.Models;

namespace NewMyApp.Core.Services;

public interface ICertificateService
{
    Task<byte[]> GenerateCertificateAsync(string userId);
    Task<bool> IsEligibleForCertificateAsync(string userId);
} 