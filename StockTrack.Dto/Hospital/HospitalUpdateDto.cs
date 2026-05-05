using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StockTrack.Dto.Hospital
{
    public class HospitalUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string? HbysName { get; set; }
        public string? HbysVersion { get; set; }
        public bool IsActive { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string? SnUsername { get; set; }
        public string? SnPassword { get; set; }
    }
}
