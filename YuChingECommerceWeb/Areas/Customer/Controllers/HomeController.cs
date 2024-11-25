using YuChingECommerce.DataAccess.Repository.IRepository;
using YuChingECommerce.Models;
using YuChingECommerce.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Text;
using YuChingECommerceWeb.Areas.Customer.Services.Interfaces;

namespace YuChingECommerceWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ChineseConverterService _chineseConverter;
        private readonly IHomeService _homeService;

        public HomeController(
              ILogger<HomeController> logger
            , IUnitOfWork unitOfWork
            , IHttpClientFactory httpClientFactory
            , ChineseConverterService chineseConverterService
            , IHomeService homeService
            )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _chineseConverter = chineseConverterService;
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
        {            
            return View(await _homeService.Index());
        }

        public async Task<IActionResult> Details(int productId)
        {            
            return View(await _homeService.Details(productId));
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId &&
            u.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                //shopping cart exists
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
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
        [IgnoreAntiforgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> GetFortune(string zodiacSign, string birthDate)
        {
            try
            {
                if (DateTime.TryParse(birthDate, out DateTime parsedDate))
                {
                    string sign = zodiacSign.ToLower();

                    // 從 Horoscope App API 獲取每日運勢（簡體中文）
                    HoroscopeApiResponse simplifiedHoroscope = await FetchDailyHoroscope(sign);

                    if (null == simplifiedHoroscope)
                    {
                        Response.StatusCode = 500;
                        return Json(new { error = "無法取得星座運勢資訊。" });
                    }

                    // 將簡體中文翻譯為繁體中文
                    string traditionalHoroscope = _chineseConverter.ConvertToTraditional(simplifiedHoroscope.Data.FortuneText.All.ToString());

                    if (string.IsNullOrEmpty(traditionalHoroscope))
                    {
                        Response.StatusCode = 500;
                        return Json(new { error = "無法轉換星座運勢資訊。" });
                    }

                    // 繼續處理幸運色和推薦商品
                    //string luckyColorYear = ZodiacHelper.GetLuckyColor(zodiacSign, DateTime.Now);
                    //string luckyColorMonth = ZodiacHelper.GetLuckyColor(zodiacSign, DateTime.Now);

                    //string luckyColorYear = simplifiedHoroscope.Data.LuckyColor;
                    string luckyColorMonth = simplifiedHoroscope.Data.LuckyColor;

                    var products = _unitOfWork.Product.GetAll(
                        p => /*p.Color == luckyColorYear ||*/ p.Color == luckyColorMonth,
                        includeProperties: "ProductImages"
                    ).ToList();

                    var result = new
                    {
                        //luckyColorYear,
                        luckyColorMonth,
                        horoscope = traditionalHoroscope,
                        products = products.Select(p => new
                        {
                            p.Id,
                            p.Title,
                            ImageUrl = p.ProductImages?.FirstOrDefault()?.ImageUrl ?? "/images/default_product.png"
                        }).ToList()
                    };

                    Console.WriteLine("幸運色Result: " + JsonConvert.SerializeObject(result));

                    return Json(result);
                }
                else
                {
                    Response.StatusCode = 400;
                    return Json(new { error = "生日格式無效。" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in GetFortune: " + ex.Message);
                return Json(new { error = "伺服器發生錯誤。" });
            }
        }

        // 從 Horoscope App API 獲取每日運勢（簡體中文）
        private async Task<HoroscopeApiResponse> FetchDailyHoroscope(string sign)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // 構建請求 URL
                string url = $"https://api.vvhan.com/api/horoscope/?type={sign}&time=today";

                // 如果 API 需要發送 JSON 數據，創建 JSON 內容
                var requestData = new
                {
                    // 根據 API 的需求填寫必要的數據
                    // 例如：Type = sign, Time = "today"
                };

                // 序列化數據為 JSON 字符串
                //string jsonSerialize = System.Text.Json.JsonSerializer.Serialize(requestData);

                // 創建 StringContent 並設置 Content-Type 為 application/json
                var content = new StringContent("", Encoding.UTF8, "application/json");

                // 發送 POST 請求，並不需要傳送任何內容
                var response = await client.PostAsync(url, content);

                //if (response.IsSuccessStatusCode)
                //{
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var horoscopeResponse = System.Text.Json.JsonSerializer.Deserialize<HoroscopeApiResponse>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (horoscopeResponse != null && horoscopeResponse.Success && horoscopeResponse.Data != null)
                    {
                        // 返回 'fortunetext.all' 作為詳細運勢描述
                        return horoscopeResponse;
                    }
                    else
                    {
                        Console.WriteLine("Horoscope App API 回應中 'success' 為 false 或 'data' 為 null。");
                    }
                //}
                //else
                //{
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Horoscope App API 返回錯誤狀態碼: {response.StatusCode}, 內容: {errorContent}");
                //}

                // 如果到這裡，表示出現問題
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in FetchDailyHoroscope: " + ex.Message);
                return null;
            }
        }

        // 使用 Google api 將簡體中文翻譯為繁體中文
        private async Task<string> TranslateTextToTraditionalChinese(string text)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                string apiKey = "";//_configuration["ApiKeys:GoogleTranslate"]; // 從配置讀取 API 金鑰
                string url = $"https://translation.googleapis.com/language/translate/v2?key={apiKey}";

                var requestBody = new
                {
                    q = text,
                    target = "zh-TW",
                    format = "text"
                };

                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var jsonDoc = JsonDocument.Parse(jsonString);

                    if (jsonDoc.RootElement.TryGetProperty("data", out JsonElement dataElement) &&
                        dataElement.TryGetProperty("translations", out JsonElement translationsElement) &&
                        translationsElement[0].TryGetProperty("translatedText", out JsonElement translatedTextElement))
                    {
                        return translatedTextElement.GetString();
                    }
                    else
                    {
                        Console.WriteLine("翻譯 API 回應中缺少 'translatedText' 欄位。");
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"翻譯 API 返回錯誤狀態碼: {response.StatusCode}, 內容: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in TranslateTextToTraditionalChinese: " + ex.Message);
                return null;
            }
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