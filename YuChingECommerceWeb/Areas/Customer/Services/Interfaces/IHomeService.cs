using Microsoft.AspNetCore.Mvc;
using YuChingECommerce.Models;

namespace YuChingECommerceWeb.Areas.Customer.Services.Interfaces
{
    public interface IHomeService
    {
        Task<IEnumerable<Product>> Index();
        public Task<ShoppingCart> Details(int productId);       
    }
}
