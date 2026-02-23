using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Abstract
{
    public interface IHospitalService : IGenericService<Hospital>
    {
        Task TAddAsync(Hospital hospital);
    }
}
