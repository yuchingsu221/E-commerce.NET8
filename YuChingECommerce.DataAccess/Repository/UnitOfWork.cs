using YuChingECommerce.DataAccess.Repository.IRepository;
using YuChingECommerce.DataAcess.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace YuChingECommerce.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed;
        private ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public ICompanyRepository Company { get; private set; }
        public IProductRepository Product { get; private set; }
        public IShoppingCartRepository ShoppingCart { get; private set; }
        public IApplicationUserRepository ApplicationUser { get; private set; }
        public IOrderHeaderRepository OrderHeader { get; private set; }
        public IOrderDetailRepository OrderDetail { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            ProductImage = new ProductImageRepository(_db);
            ApplicationUser = new ApplicationUserRepository(_db);
            ShoppingCart = new ShoppingCartRepository(_db);
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            Company = new CompanyRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
        }

        public void Save()
        {
            _db.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _db.Database.BeginTransaction();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _db.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            if (!_disposed && _db != null)
            {
                _disposed = true;
                _db.Dispose();
            }
        }
    }
}
