using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.EntityFramework
{
    public class EfProductDal : GenericRepository<Product>, IProductDal
    {
        public EfProductDal(AppDbContext context) : base(context)
        {
        }
    }
}
