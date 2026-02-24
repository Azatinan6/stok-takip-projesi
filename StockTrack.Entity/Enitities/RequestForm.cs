namespace StockTrack.Entity.Enitities
{
    public class RequestForm : EntityBase
    {
        public int MainRepoLocationId { get; set; }
        public int LocationListId { get; set; }
        public int RequestFormTypeId { get; set; }

        // --- YENİ EKLENEN EK SEÇENEKLER (Özet Ekranı) ---
        public bool IsShipAfterReturn { get; set; } // İade Ürünler Geldiği Zaman Kargolanacak
        public bool IsOfficeDelivery { get; set; }  // Ofisten Teslim Edilecek
    }
}
