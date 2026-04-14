using StockTrack.Dto.RequestForm;

namespace StockTrack.Dto.CargoDetail
{
    public class ResultAwitingApprovalDto
    {
        public int Id { get; set; }
        //public List<string> CategoryNames { get; set; }
        public int CargoNameId { get; set; }
        public string? CargoProccessBy { get; set; } //Kargo işlemlerini yapan kişi
        public DateTime? PackingDate { get; set; } //Kargo paketlenme tarihi
        public DateTime? CompletedCargoDate { get; set; } //Kargoya teslim edilme tarihi
        public string? CargoCompany { get; set; }
        public string? ReceiverFullName { get; set; }
        public string? Phone { get; set; }
        //public List<string> ProductNames { get; set; }
        public string? HospitalName { get; set; }
        public string? HospitalAddress { get; set; }
        public string? MainRepoName { get; set; } 
        //public List<int> Quantities { get; set; }
        public DateTime? RequestFormRequestedDate { get; set; } //Talebi oluşturup onaylanma  tarihi
        public string? RequestFormRequestedBy { get; set; } //Talebi oluşturup onaylayan kişi
        //public List<string>? PhotoUrls { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }

        public DateTime? CargoGivenDate { get; set; }
        public bool IsOfficeDelivery { get; set; }
        public string? TrackingNumber { get; set; }

        // Gönderim Detayları
        public string? Label { get; set; }
        public string? SendReason { get; set; }
        public string? ProductCondition { get; set; }
        public string? Note { get; set; }

        // Arc Box Teknik Detayları
        public string? SerialNumber { get; set; }
        public string? EthMac { get; set; }
        public string? WlanMac { get; set; }
        public string? ConnectionType { get; set; }
        public string? ConfigUrl { get; set; }  

        public List<ProductDetailDto> Products { get; set; }
    }
}
