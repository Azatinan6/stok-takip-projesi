using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockTrack.Dto.RequestForm
{
    public class SaveReturnInfoDto
    {
        public int RequestFormId { get; set; }
        public bool IsWastage { get; set; }
        public string ReturnReason { get; set; }
        public string ReturnedSerialNumber { get; set; }
    }
}
