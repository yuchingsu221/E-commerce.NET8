using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace YuChingECommerce.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }
        IShoppingCartRepository ShoppingCart { get; }
        IApplicationUserRepository ApplicationUser { get; }
        IOrderDetailRepository OrderDetail { get; }
        IOrderHeaderRepository OrderHeader { get; }
        IProductImageRepository ProductImage { get; }
        void Save();
        Task SaveAsync();
        Task SaveChangesAsync();
        void SaveChanges();
        IDbContextTransaction BeginTransaction();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
