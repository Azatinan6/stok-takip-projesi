namespace StockTrack.Dto.MainRepoLocation
{
    //Depoda bulunan ürünlerin dağıtılan bilgisi
    public class StockDetailDto
    {
        public string MainRepoName { get; set; }
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }

        
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Persons { get; set; }

        // Diğer tekil özellikler aynı kalır
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? RequestBy { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Description { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Adress { get; set; }
        public string? ToPerson { get; set; }
        public string? Phone { get; set; }
        public DateTime? PackingDate { get; set; }
        public DateTime? CargoGivenDate { get; set; }
        public string? CargoName { get; set; }
        public DateTime? InstallationDate { get; set; }
        public string StatusName { get; set; }
        public string RequestFormType { get; set; }
    }

}
