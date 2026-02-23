using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Concrete
{
    public class LocationListManager : GenericManager<LocationList>, ILocationListService
    {
        public LocationListManager(IGenericDal<LocationList> genericDal) : base(genericDal)
        {
        }
    }
}
