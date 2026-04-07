using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.EntityFramework
{
    public class EfHospitalDal : GenericRepository<Hospital>, IHospitalDal
    {

        public EfHospitalDal(AppDbContext context) : base(context)
        {
        }
    }
}
