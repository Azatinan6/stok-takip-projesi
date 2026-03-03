using StockTrack.Dto.RequestForm;

namespace StockTrack.Dto.CargoDetail
{
    public class ResultCargoInDeliveryDto
    {
        public int Id { get; set; }
        public List<string> CategoryNames { get; set; }
        public string CargoCompany { get; set; }
        public string TrakingNumber { get; set; }
        public string ReceiverFullName { get; set; }
        public string Phone { get; set; }
        //public List<string> ProductNames { get; set; }
        public string LocationName { get; set; }
        public string LocationAdress { get; set; }
        public string MainRepoName { get; set; }
        //public List<int> Quantities { get; set; }
        public DateTime RequestFormDate { get; set; } //Talebi oluşturulma tarihi
        public string RequestFormBy { get; set; } //Talebi oluşturan kişi
        //public List<string>? PhotoUrls { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public List<ProductDetailDto> Products { get; set; }
    }
}
