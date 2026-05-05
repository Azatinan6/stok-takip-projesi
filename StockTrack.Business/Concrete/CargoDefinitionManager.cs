using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.Entity.Enitities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockTrack.Business.Concrete
{
    public class CargoDefinitionManager : ICargoDefinitionService
    {
        private readonly ICargoDefinitionDal _cargoDefinitionDal;

        public CargoDefinitionManager(ICargoDefinitionDal cargoDefinitionDal)
        {
            _cargoDefinitionDal = cargoDefinitionDal;
        }

        public async Task TCreateAsync(CargoDefinition entity) => await _cargoDefinitionDal.AddAsync(entity);
        public async Task TCreateRangeAsync(IEnumerable<CargoDefinition> entities) => await _cargoDefinitionDal.AddRangeAsync(entities);
        public async Task TDeleteAsync(int id) => await _cargoDefinitionDal.DeleteAsync(id);
        public async Task<List<CargoDefinition>> TGetFilteredListAsync(Expression<Func<CargoDefinition, bool>> filter) => await _cargoDefinitionDal.GetFilteredListAsync(filter);
        public async Task<CargoDefinition> TGetByIdAsync(int id) => await _cargoDefinitionDal.GetByIdAsync(id);
        public async Task<List<CargoDefinition>> TGetListAsync() => await _cargoDefinitionDal.GetListAsync();
        public async Task TUpdateAsync(CargoDefinition entity) => await _cargoDefinitionDal.UpdateAsync(entity);
        public async Task TUpdateRangeAsync(List<CargoDefinition> entities) => await _cargoDefinitionDal.UpdateRangeAsync(entities);
        public async Task<int> TCountAsync() => await _cargoDefinitionDal.CountAsync();
    }
}