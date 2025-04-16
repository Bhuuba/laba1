using Microsoft.AspNetCore.Mvc;

namespace NewMyApp.Web.Controllers
{
    public class DocumentationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PdfGuide()
        {
            return View();
        }
    }
}