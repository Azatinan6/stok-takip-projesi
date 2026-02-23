using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.EntityFramework
{
    public class EfCargoDefinitionDal : GenericRepository<CargoDefinition>, ICargoDefinitionDal
    {
        public EfCargoDefinitionDal(AppDbContext context) : base(context)
        {
        }
    }
}