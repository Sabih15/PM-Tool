using DAL.Models;
using Microsoft.EntityFrameworkCore;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        private readonly PMToolDbContext dbContext;
        private readonly DbSet<T> table = null;
        readonly Type type = typeof(T);
        public Repository(PMToolDbContext _pMToolDbContext)
        {
            dbContext = _pMToolDbContext;
            table = dbContext.Set<T>();
        }

        public virtual IQueryable<T> GetAll()
        {
            return table.Where(a => a.IsActive == true).AsQueryable<T>();
        }
        public async Task<IList<T>> GetAllListAsync()
        {
            return await table.Where(a => a.IsActive == true).ToListAsync();
        }
        public virtual async Task<T> Get(int id)
        {
            return await table.FindAsync(id);
        }
        public virtual T Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot insert empty object.");
            }
            entity.CreatedDate = DateTime.Now;
            entity.IsActive = true;
            table.Add(entity);
            dbContext.SaveChanges();
            return entity;
        }
        public virtual async Task<T> InsertAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot insert empty object.");
            }
            entity.CreatedDate = DateTime.Now;
            entity.IsActive = true;
            table.Add(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public virtual int InsertAndGetId(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot insert empty object.");
            }
            entity.CreatedDate = DateTime.Now;
            entity.IsActive = true;
            table.Add(entity);
            dbContext.SaveChanges();
            var idName = dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Single().Name;
            var val = (int)entity.GetType().GetProperty(idName).GetValue(entity, null);
            return val;
        }
        public virtual async Task<int> InsertAndGetIdAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot insert empty object.");
            }
            entity.CreatedDate = DateTime.Now;
            entity.IsActive = true;
            table.Add(entity);
            await dbContext.SaveChangesAsync();
            var idName = dbContext.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Single().Name;
            var val = (int)entity.GetType().GetProperty(idName).GetValue(entity, null);
            return val;
        }
        public virtual T Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot update empty object.");
            }
            entity.UpdatedDate = DateTime.Now;
            table.Update(entity);
            dbContext.SaveChanges();
            return entity;
        }
        public virtual async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("Cannot update empty object.");
            }
            entity.UpdatedDate = DateTime.Now;
            table.Update(entity);
            await dbContext.SaveChangesAsync();
            return entity;
        }
        public virtual void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entity.IsActive = false;
            Update(entity);
            dbContext.SaveChanges();
        }
        public virtual void HardDelete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            table.Remove(entity);
            dbContext.SaveChanges();
        }
        public virtual void HardDeleteRange(List<T> entities)
        {
            if (entities.Count <= 0)
            {
                throw new ArgumentNullException("entity");
            }
            table.RemoveRange(entities);
            dbContext.SaveChanges();
        }
    }
}
