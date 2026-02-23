namespace StockTrack.Entity.Enitities
{
    public class ProductSerialNumber : EntityBase
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int MainRepoLocationId { get; set; }
        public MainRepoLocation MainRepoLocation { get; set; }

        // Arc Box Özel Bilgileri
        public string SerialNumber { get; set; }
        public string? EthMac { get; set; }
        public string? WlanMac { get; set; }

        public int ProductStatusId { get; set; } // Yeni, Kullanılmış vs.
        public string? Description { get; set; }
    }
}