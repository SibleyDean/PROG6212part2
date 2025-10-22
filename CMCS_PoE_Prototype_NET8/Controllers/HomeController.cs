using Microsoft.AspNetCore.Mvc;

namespace CMCS.Mvc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Claims");
        }

        public IActionResult Error() => View();
    }
}