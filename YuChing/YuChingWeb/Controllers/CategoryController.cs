using Microsoft.AspNetCore.Mvc;

namespace YuChingWeb.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
