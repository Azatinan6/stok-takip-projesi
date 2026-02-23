using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Concrete
{
    public class CategoryManager : GenericManager<Category>, ICategoryService
    {
        public CategoryManager(IGenericDal<Category> genericDal) : base(genericDal)
        {
        }
              
    }
}
