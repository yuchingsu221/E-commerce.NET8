using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using YuChingECommerce.DataAccess.Repository.IRepository;
using YuChingECommerce.DataAcess.Data;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using YuChingECommerce.Models.BaseEntity;

namespace YuChingECommerce.DataAccess.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ApplicationDbContext _db;
        internal DbSet<TEntity> dbSet;
        public Repository(ApplicationDbContext db)
        {
            _db = db;
            this.dbSet = _db.Set<TEntity>();
            //_db.Categories == dbSet
            _db.Products.Include(u => u.Category).Include(u => u.CategoryId);

        }

        public void Add(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<TEntity> query;
            if (tracked)
            {
                query = dbSet;

            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();

        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<TEntity> query;
            if (tracked)
            {
                query = dbSet;

            }
            else
            {
                query = dbSet.AsNoTracking();
            }

            query = query.Where(filter);
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.FirstOrDefaultAsync();
        }

        public IEnumerable<TEntity> GetAll(Expression<Func<TEntity, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.ToList();
        }

        public async Task<ICollection<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<TEntity> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProp in includeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return await query.ToListAsync();
        }

        public void Remove(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<TEntity> entity)
        {
            dbSet.RemoveRange(entity);
        }

        public async Task<ICollection<TEntity>> ToListAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await dbSet.FirstOrDefaultAsync(predicate);
        }

        //public async Task<int> InsertAsync(TEntity entity)
        //{
            //if (entity == null) throw new ArgumentNullException("entity");

            //await dbSet.AddAsync(entity);
            //// 上方的add async 並不會直接新增到dbSet
            //var lastEntity = await dbSet.OrderBy(x => x.Id).LastOrDefaultAsync();
            //return lastEntity is null ? 1 : lastEntity.Id + 1;
        //}

        public void Insert(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            dbSet.Add(entity);
        }

        public async Task InsertMultipleAsync(IEnumerable<TEntity> newEntities)
        {
            if (newEntities == null) throw new ArgumentNullException("newEntities");

            await dbSet.AddRangeAsync(newEntities);
        }

        //public async Task RemoveByIdAsync(int id)
        //{
        //    TEntity entity = await dbSet.FirstOrDefaultAsync(s => s.Id == id);
        //    dbSet.Remove(entity);
        //}

        public Task RemoveMultipleAsync(IEnumerable<TEntity> removeEntities)
        {
            if (removeEntities == null) throw new ArgumentNullException("removeEntities");
            dbSet.RemoveRange(removeEntities);
            return Task.CompletedTask;
        }

        //public Task RemoveMultipleByIdAsync(IEnumerable<int> ids)
        //{
        //    var removeEntities = dbSet.Where(e => ids.Contains(e.Id));
        //    dbSet.RemoveRange(removeEntities);
        //    return Task.CompletedTask;
        //}

        public Task UpdateAsync(TEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdateMultipleAsync(IEnumerable<TEntity> newEntities)
        {
            if (newEntities == null) throw new ArgumentNullException("newEntities");
            dbSet.UpdateRange(newEntities);
            return Task.CompletedTask;
        }
    }
}
