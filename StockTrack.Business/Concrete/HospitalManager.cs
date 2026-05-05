using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Concrete
{
    public class HospitalManager : GenericManager<Hospital>, IHospitalService
    {
        private readonly IHospitalDal _hospitalDal;
        public HospitalManager(IHospitalDal hospitalDal) : base(hospitalDal)
        {
            _hospitalDal = hospitalDal;
        }

        public async Task TAddAsync(Hospital hospital)
        {
            await _hospitalDal.AddAsync(hospital);
        }
    }
}
