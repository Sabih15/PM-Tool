using DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        void Delete(T entity);
        void HardDelete(T entity);
        Task<T> Get(int id);
        IQueryable<T> GetAll();
        Task<IList<T>> GetAllListAsync();
        T Insert(T entity);
        Task<T> InsertAsync(T entity);
        int InsertAndGetId(T entity);
        Task<int> InsertAndGetIdAsync(T entity);
        Task<T> UpdateAsync(T entity);
        T Update(T entity);
        void HardDeleteRange(List<T> entities);
    }
}