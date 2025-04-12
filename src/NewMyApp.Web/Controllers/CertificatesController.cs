using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NewMyApp.Core.Services;
using System.Threading.Tasks;

namespace NewMyApp.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class CertificatesController : Controller
    {
        private readonly ICertificateService _certificateService;

        public CertificatesController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        [HttpGet("Generate")]
        public async Task<IActionResult> Generate()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var pdfBytes = await _certificateService.GenerateCertificateAsync(userId);
            if (pdfBytes == null)
                return NotFound("You are not eligible for a certificate yet. You need at least 10 likes on your posts.");

            return File(pdfBytes, "application/pdf", $"certificate_{userId}.pdf");
        }
    }
} 