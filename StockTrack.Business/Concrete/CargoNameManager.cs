using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.Entity.Enitities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockTrack.Business.Concrete
{
    public class CargoNameManager : ICargoNameService
    {
        private readonly ICargoNameDal _cargoNameDal;

        public CargoNameManager(ICargoNameDal cargoNameDal)
        {
            _cargoNameDal = cargoNameDal;
        }

        // AddAsync yerine senin IGenericDal'ındaki doğru ismi (AddAsync) kullandığından emin ol
        public async Task TCreateAsync(CargoName entity) => await _cargoNameDal.AddAsync(entity);

        public async Task TCreateRangeAsync(IEnumerable<CargoName> entities) => await _cargoNameDal.AddRangeAsync(entities);

        // HATA BURADAYDI: Nesne yerine sadece ID gönderiyoruz
        public async Task TDeleteAsync(int id) => await _cargoNameDal.DeleteAsync(id);

        public async Task<List<CargoName>> TGetFilteredListAsync(Expression<Func<CargoName, bool>> filter) => await _cargoNameDal.GetFilteredListAsync(filter);

        public async Task<CargoName> TGetByIdAsync(int id) => await _cargoNameDal.GetByIdAsync(id);

        public async Task<List<CargoName>> TGetListAsync() => await _cargoNameDal.GetListAsync();

        public async Task TUpdateAsync(CargoName entity) => await _cargoNameDal.UpdateAsync(entity);

        public async Task TUpdateRangeAsync(List<CargoName> entities) => await _cargoNameDal.UpdateRangeAsync(entities);

        public async Task<int> TCountAsync() => await _cargoNameDal.CountAsync();
    }
}