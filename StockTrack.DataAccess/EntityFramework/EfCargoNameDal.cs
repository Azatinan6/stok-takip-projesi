using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities; // Klasör ismine dikkat (Enitities)

namespace StockTrack.DataAccess.EntityFramework
{
    public class EfCargoNameDal : GenericRepository<CargoName>, ICargoNameDal
    {
        // Tüm metodlar (AddAsync vb.) GenericRepository'den otomatik geleceği için 
        // burada tekrar tanımlamana GEREK YOKTUR.

        public EfCargoNameDal(AppDbContext context) : base(context)
        {
        }
    }
}