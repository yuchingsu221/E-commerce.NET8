using YuChingECommerce.DataAccess.Repository.IRepository;
using YuChingECommerce.Models;
using YuChingECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace YuChingECommerceWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new() {
                Product = _unitOfWork.Product.Get(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                Count = 1,
                ProductId = productId
            };
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart) 
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId= userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId == userId &&
            u.ProductId==shoppingCart.ProductId);

            if (cartFromDb != null) {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else {
                //add cart record
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart,
                _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count());
            }
            TempData["success"] = "Cart updated successfully";

           


            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult GetFortune(string zodiacSign, string birthDate)
        {
            if (DateTime.TryParse(birthDate, out DateTime parsedDate))
            {
                string luckyColorYear = ZodiacHelper.GetLuckyColor(zodiacSign, DateTime.Now);
                string luckyColorMonth = ZodiacHelper.GetLuckyColor(zodiacSign, DateTime.Now); // 可根據月份進一步細分
                string horoscope = ZodiacHelper.GetDailyHoroscope(zodiacSign);

                // 獲取對應幸運色的商品
                var products = _unitOfWork.Product.GetAll(p => p.Color == luckyColorYear || p.Color == luckyColorMonth).ToList();

                // 構建返回的資料
                var result = new
                {
                    LuckyColorYear = luckyColorYear,
                    LuckyColorMonth = luckyColorMonth,
                    Horoscope = horoscope,
                    Products = products.Select(p => new
                    {
                        p.Id,
                        p.Title,
                        ImageUrl = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/default_product.png"
                    })
                };

                return Json(result);
            }

            return BadRequest("Invalid date format.");
        }

        public IActionResult LuckyBracelets(string birthDate)
        {
            if (DateTime.TryParse(birthDate, out DateTime parsedDate))
            {
                string zodiacSign = LuckyColorHelper.GetZodiacSign(parsedDate);
                string luckyColor = LuckyColorHelper.GetLuckyColor(zodiacSign, DateTime.Now);

                var products = _unitOfWork.Product.GetAll(p => p.Color == luckyColor);

                ViewData["ZodiacSign"] = zodiacSign;
                ViewData["LuckyColor"] = luckyColor;

                return View(products);
            }

            return BadRequest("Invalid date format.");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}