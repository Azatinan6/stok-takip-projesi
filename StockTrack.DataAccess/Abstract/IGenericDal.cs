using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockTrack.DataAccess.Abstract
{
    public interface IGenericDal<T> where T : class
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities); // Bu metod da olmalı
        Task DeleteAsync(int id); // Veya DeleteAsync(T entity)
        Task<List<T>> GetFilteredListAsync(Expression<Func<T, bool>> filter);
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetListAsync();
        Task UpdateAsync(T entity);
        Task UpdateRangeAsync(List<T> entities);
        Task<int> CountAsync();
    }
}