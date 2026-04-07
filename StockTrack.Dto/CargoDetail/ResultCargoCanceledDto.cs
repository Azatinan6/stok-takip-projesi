using StockTrack.Dto.RequestForm;

namespace StockTrack.Dto.CargoDetail
{
    public class ResultCargoCanceledDto
    {
        public int Id { get; set; }
        //public List<string> CategoryNames { get; set; }   
        public string? CancaledBy { get; set; } //İptal eden kişi
        public string? CanceledDesc { get; set; } //iptal açıklaması
        public string ReceiverFullName { get; set; }
        public string Phone { get; set; }
        //public List<string> ProductNames { get; set; }
        //public string LocationName { get; set; }
        //public string LocationAdress { get; set; }
        public string HospitalName { get; set; }
        public string HospitalAddress { get; set; }
        public string MainRepoName { get; set; }
        //public List<int> Quantities { get; set; }
        public DateTime RequestFormDate { get; set; } //Talebi oluşturulma tarihi
        public string RequestFormBy { get; set; } //Talebi oluşturan kişi
        public DateTime? RequestFormRequestedDate { get; set; } //Talebi oluşturup onaylanma  tarihi
        public string? RequestFormRequestedBy { get; set; } //Talebi oluşturup onaylayan kişi
        //public List<string>? PhotoUrls { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }

        public DateTime? CargoGivenDate { get; set; }
        public bool IsOfficeDelivery { get; set; }
        public string? TrackingNumber { get; set; }

        public List<ProductDetailDto> Products { get; set; }

        public string CargoCompany { get; set; }
    }
}
