using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrack.Dto.CargoName
{
    public class CargoNameDeletedDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? DeletedDate { get; set; }

    }
}
