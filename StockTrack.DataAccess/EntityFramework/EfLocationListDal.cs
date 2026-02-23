using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.EntityFramework
{
    public class EfLocationListDal : GenericRepository<LocationList>, ILocationListDal
    {
        public EfLocationListDal(AppDbContext context) : base(context)
        {
        }
    }
}
