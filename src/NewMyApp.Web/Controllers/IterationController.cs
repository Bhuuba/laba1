using Microsoft.AspNetCore.Mvc;

namespace NewMyApp.Web.Controllers
{
    public class IterationController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Continue()
        {
            var confirmMessage = "Продолжить итерацию?";
            return Json(new { confirmMessage });
        }
    }
}