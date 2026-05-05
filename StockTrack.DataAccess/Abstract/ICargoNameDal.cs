using StockTrack.Entity.Enitities;

namespace StockTrack.DataAccess.Abstract
{
    // IGenericDal'dan miras aldığından ve CargoName tipini verdiğinden emin ol
    public interface ICargoNameDal : IGenericDal<CargoName>
    {
     
    }
}