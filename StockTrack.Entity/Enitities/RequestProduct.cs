namespace StockTrack.Entity.Enitities
{
    public class RequestProduct
    {
        public int Id { get; set; } // Her satırın kendi ID'si olmalı

        public int RequestFormId { get; set; }
        public RequestForm RequestForm { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

        // --- YENİ EKLENEN ALANLAR ---
        public int OperationType { get; set; } // 1: Gönderilecek, 2: İade Beklenen
        public int? ReasonId { get; set; } // Neden (Eğer ID olarak tutacaksan)
        public string? Label { get; set; } // Etiket (Cihaz isimleri vs.)

        // --- ARC BOX CONFIG ALANLARI ---
        public string? ConnectionType { get; set; }
        public string? ConfigUrl { get; set; }
        public string? DhcpdConf { get; set; }
        public string? WpaSupplicantConf { get; set; }
    }
}