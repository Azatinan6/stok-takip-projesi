using System.Linq.Expressions;

namespace StockTrack.Business.Abstract
{
    public interface IGenericService<T> where T : class
    {
        Task TCreateAsync(T entity);
        Task TCreateRangeAsync(IEnumerable<T> entities);
        Task TUpdateAsync(T entity);
        Task TUpdateRangeAsync(List<T> entity);
        Task TDeleteAsync(int id);
        Task<List<T>> TGetListAsync();
        Task<T> TGetByIdAsync(int id);
        Task<int> TCountAsync();
        Task<List<T>> TGetFilteredListAsync(Expression<Func<T, bool>> predicate);
    }
}
