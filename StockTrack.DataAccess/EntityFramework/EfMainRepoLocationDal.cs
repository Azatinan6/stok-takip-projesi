using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.EntityFramework
{
    public class EfMainRepoLocationDal : GenericRepository<MainRepoLocation>, IMainRepoLocationDal
    {
        public EfMainRepoLocationDal(AppDbContext context) : base(context)
        {
        }
    }
}
