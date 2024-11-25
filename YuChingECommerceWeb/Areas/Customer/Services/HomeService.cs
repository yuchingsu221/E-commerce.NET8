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

namespace YuChingECommerceWeb.Areas.Customer.Services
{
    [Area("Customer")]
    public class HomeService : IHomeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ChineseConverterService _chineseConverter;

        public HomeService(IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, ChineseConverterService chineseConverterService)
        {
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _chineseConverter = chineseConverterService;
        }

        public async Task<IEnumerable<Product>> Index()
        {
            IEnumerable<Product> productList = await _unitOfWork.Product.GetAllAsync(includeProperties: "Category,ProductImages");
            return productList;
        }

        public async Task<ShoppingCart> Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = await _unitOfWork.Product.GetAsync(u => u.Id == productId, includeProperties: "Category,ProductImages"),
                Count = 1,
                ProductId = productId
            };
            return cart;
        }       
    }
}