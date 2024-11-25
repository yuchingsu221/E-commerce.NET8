using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace YuChingECommerce.DataAccess.Repository.IRepository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        //T - Category
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>>? filter = null, string? includeProperties = null);
        Task<ICollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null, string? includeProperties = null);
        TEntity Get(Expression<Func<TEntity, bool>> filter, string? includeProperties = null, bool tracked = false);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter, string? includeProperties = null, bool tracked = false);
        void Add(TEntity entity);
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entity);
        Task<ICollection<TEntity>> ToListAsync();
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        //Task<int> InsertAsync(TEntity item);
        void Insert(TEntity item);
        Task InsertMultipleAsync(IEnumerable<TEntity> items);
        Task UpdateAsync(TEntity item);
        Task UpdateMultipleAsync(IEnumerable<TEntity> items);
        //public Task RemoveMultipleByIdAsync(IEnumerable<int> ids);
        //Task RemoveByIdAsync(int id);
        Task RemoveMultipleAsync(IEnumerable<TEntity> items);
    }
}
