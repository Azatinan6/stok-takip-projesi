namespace StockTrack.Dto.LocationList
{
    public class ProductDetailsForLocationDto
    {
        public string MainRepoName { get; set; }
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
       
        public List<string> Persons { get; set; }

        // Diğer tekil özellikler aynı kalır
        public string CreatedBy { get; set; }
        public string? CreatedDate { get; set; }
        public string? RequestBy { get; set; }
        public string? RequestDate { get; set; }
        public string? ApprovalDate { get; set; }
        public string? CompletedDate { get; set; }
        public string? Description { get; set; }
        public string? TrackingNumber { get; set; }
        public string? Adress { get; set; }
        public string? ToPerson { get; set; }
        public string? Phone { get; set; }
        public string? PackingDate { get; set; }
        public string? CargoGivenDate { get; set; }
        public string? CargoName { get; set; }
        public string? InstallationDate { get; set; }
        public string StatusName { get; set; }
        public string RequestFormType { get; set; }
        public List<ProductDetailsLocationVM> Products { get; set; }

    }
    public class ProductDetailsLocationVM
    {
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public int UsedQuantity { get; set; }
    }
}
