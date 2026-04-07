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

        public string? IpAddress { get; set; }
        public string? EthMacAddress { get; set; }
        public string? WlanMacAddress { get; set; }

        // Ürün Durumu (Yeni, Kullanılmış, Arızalı vs. - Tanımlardan gelen ID)
        public int? ProductStatusId { get; set; }

        // --- PDF: İade Süreci Özel Alanları ---
        // Teslim Alınan Adet (Gönderilen ile gelen farklı olabilir)
        public int? ReceivedQuantity { get; set; }

        // Kontrol Sonucu (Tanımlardan gelecek ID)
        public int? ControlResultId { get; set; }

        // --- PDF: Zayiat (Fire) ve Seri No Kontrolü ---
        // Bu ürün iadede zayiat mı oldu?
        public bool IsWaste { get; set; }

        // Zayiatın açıklaması
        public string? WasteDescription { get; set; }

        // ArcBox için: Gelen ürünün seri nosu sistemdekiyle aynı mı?
        public bool? IsSerialNumberMatched { get; set; }

        // Eğer uyuşmuyorsa, sahadan gelen yeni seri numarası
        public string? ReceivedSerialNumber { get; set; }
    }
}