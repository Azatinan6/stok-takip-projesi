namespace StockTrack.Dto.RequestForm
{
    public class RequestedForRequestFormDto
    {
        public int RequestFormDetailId { get; set; }
        public int RequestFormTypeId { get; set; }
        public string RequestTypeName { get; set; }
        public int RequestStatusId { get; set; }
        public string MainRepoLocationName { get; set; }
        public string RequestBy { get; set; }
        public DateTime? RequestDate { get; set; }

        // --- Lokasyon Yerine Hastane Bilgileri ---
        public string HospitalName { get; set; } // Eski 'Location'
        public string HospitalAddress { get; set; } // Eski 'Address'

        // --- Kargo Detayları ---
        public string ReceiverName { get; set; }
        public string Phone { get; set; }

        // --- Kurulum & Servis Detayları ---
        public DateTime? InstallationDate { get; set; }
        public string? Description { get; set; }
        public List<string> Persons { get; set; }

        // --- Ürün Listesi ---
        public List<ProductDetailDto> Products { get; set; }
    }
}