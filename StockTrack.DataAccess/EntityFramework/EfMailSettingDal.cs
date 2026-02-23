using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.DataAccess.Repositories;
using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.EntityFramework
{
    public class EfMailSettingDal : GenericRepository<MailSetting>, IMailSettingDal
    {
        public EfMailSettingDal(AppDbContext context) : base(context)
        {
        }
    }
}
