using Microsoft.AspNetCore.Mvc;
using YuChingECommerce.Utility;

namespace YuChingECommerceWeb.Controllers
{
    public class FortuneController : Controller
    {
        public IActionResult GetFortune(string birthDate)
        {
            if (DateTime.TryParse(birthDate, out DateTime date))
            {
                string zodiacSign = ZodiacHelper.GetZodiacSign(date);
                return Json(new { zodiacSign });
            }

            return BadRequest("Invalid date format.");
        }
    }
}
