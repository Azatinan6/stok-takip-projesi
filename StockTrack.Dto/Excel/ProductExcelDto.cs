namespace StockTrack.Dto.Excel
{
    public class ProductExcelDto
    {
        public string RequestedBy { get; set; }
        public string? RequestDate { get; set; }
        public string MainDepo { get; set; }
       
        public string CategoryName { get; set; } 
        public string ProductName { get; set; }
        public int Quantitiy { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string ReciverName { get; set; }
        

        // Cargo details
        public string ToPerson { get; set; }//ilgili personeller
        public string TrackingNumber { get; set; }
        public string CargoCompany { get; set; }
        public string? PackedDate { get; set; }
        public string CargoGivenDate { get; set; } //kargoya teslim tarihi
        public string CompletedCargoDate { get; set; }

        // Installation details
        public string? InstallationDate { get; set; }
        public string InstallationNote { get; set; }
        public string RelevantPerson { get; set; }//ilgili personeller

        public string Status { get; set; }
        public int RequestTypeId { get; set; }

    }

}

