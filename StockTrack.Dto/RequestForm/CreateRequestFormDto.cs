namespace StockTrack.Dto.RequestForm
{
    public class CreateRequestFormDto
    {
        public int LocationId { get; set; }
        public int MainRepoId { get; set; }
        public int TypeId { get; set; } // 1=Kurulum, 2=Kargo, 3 servis
        public string ItemsJson { get; set; } // Hidden’dan gelecek
        public List<ItemDto>? Items { get; set; }
        public DateTime? InstallationDate { get; set; }

        //kurulum Servis
        public List<int>? Persons { get; set; }
        public string? Note { get; set; }

        //Kargo
        public string? ReceiverFullName { get; set; }
        public string? Phone { get; set; }

        // --- PDF'TE İSTENEN YENİ EK SEÇENEKLER ---
        public bool IsShipAfterReturn { get; set; } // İade Ürünler Geldiği Zaman Kargolanacak
        public bool IsOfficeDelivery { get; set; }  // Ofisten Teslim Edilecek
    }

    public class ItemDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; } // JS'den JSON ile geliyor
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; } // JS'den JSON ile geliyor
        public int Quantity { get; set; }

        // --- PDF'TE İSTENEN YENİ ÜRÜN DETAYLARI ---
        public int OperationType { get; set; } // 1: Gönderilecek, 2: İade Beklenen
        public string? Reason { get; set; } // Neden (Arıza, Yedek Parça vs.)
        public string? Label { get; set; } // Etiket bilgisi (Cihaz vs.)

        // Arc Box seçilirse içi dolacak, normal üründe null kalacak Config Objesi
        public ArcBoxConfigDto? ArcBoxConfig { get; set; }
    }

    // Arc Box modalından gelen verileri karşılayacak özel sınıf
    public class ArcBoxConfigDto
    {
        public string? ConnectionType { get; set; }
        public string? ConfigUrl { get; set; }
        public string? DhcpdConf { get; set; }
        public string? WpaSupplicantConf { get; set; }
    }
}