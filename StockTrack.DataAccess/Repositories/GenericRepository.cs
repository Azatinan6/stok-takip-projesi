using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using System.Linq.Expressions;

namespace StockTrack.DataAccess.Repositories
{
    public class GenericRepository<T> : IGenericDal<T> where T : class
    {

        private readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountAsync()
        {
            return await _context.Set<T>().CountAsync();
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var value = await GetByIdAsync(id);
            _context.Remove(value);
            await _context.SaveChangesAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> GetFilteredListAsync(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<List<T>> GetListAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRangeAsync(List<T> entity)
        {
            _context.UpdateRange(entity);
            await _context.SaveChangesAsync();
        }
    }
}
