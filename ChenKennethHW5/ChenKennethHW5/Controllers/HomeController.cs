using Microsoft.AspNetCore.Mvc;

namespace ChenKennethHW5.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}