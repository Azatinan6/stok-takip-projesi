using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using System.Linq.Expressions;

namespace StockTrack.Business.Concrete
{

    public class GenericManager<T> : IGenericService<T> where T : class
    {
        private readonly IGenericDal<T> _genericDal;

        public GenericManager(IGenericDal<T> genericDal)
        {
            _genericDal = genericDal;
        }

        public async Task<int> TCountAsync()
        {
            return await _genericDal.CountAsync();
        }

        public async Task TCreateAsync(T entity)
        {
            await _genericDal.AddAsync(entity);
        }

        public async Task TCreateRangeAsync(IEnumerable<T> entities)
        {
            await _genericDal.AddRangeAsync(entities);
        }

        public async Task TDeleteAsync(int id)
        {
            await _genericDal.DeleteAsync(id);
        }

        public async Task<T> TGetByIdAsync(int id)
        {
            return await _genericDal.GetByIdAsync(id);
        }

        public async Task<List<T>> TGetFilteredListAsync(Expression<Func<T, bool>> predicate)
        {
            return await _genericDal.GetFilteredListAsync(predicate);
        }

        public async Task<List<T>> TGetListAsync()
        {
            return await _genericDal.GetListAsync();
        }

        public async Task TUpdateAsync(T entity)
        {
            await _genericDal.UpdateAsync(entity);
        }

        public async Task TUpdateRangeAsync(List<T> entity)
        {
            await _genericDal.UpdateRangeAsync(entity);
        }
    }

}
