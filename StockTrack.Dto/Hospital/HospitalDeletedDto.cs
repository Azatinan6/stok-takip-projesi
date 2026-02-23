using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrack.Dto.Hospital
{
    public class HospitalDeletedDto 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public string? HbysName { get; set; }
        public string? HbysVersion { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsActive { get; set; }

    }
}
//#ID	Kurum Adı	Şube	HBYS	Versiyon	Silinme Tarihi	Durum