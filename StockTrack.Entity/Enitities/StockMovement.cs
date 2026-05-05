namespace StockTrack.Entity.Enitities
{
    public class StockMovement : EntityBase
    {
        // Hangi ürün ve nerede hareket gördü?
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int MainRepoLocationId { get; set; }
        public MainRepoLocation MainRepoLocation { get; set; }

        // "IN" (Giriş) veya "OUT" (Çıkış)
        public string MovementType { get; set; }

        // Stok Adetleri
        public int OldStockQuantity { get; set; } // İşlemden önceki stok
        public int NewStockQuantity { get; set; } // İşlemden sonraki stok
        public int MovementQuantity { get; set; } // Eklenen veya Çıkarılan Adet

        // Tanımlardan gelecek ID'ler (Kargo, Kurulum, Yeni, Arızalı vb.)
        public int MovementStatusId { get; set; }
        public int ProductStatusId { get; set; }

        public string? Description { get; set; }

        // Arc Box gibi seri numaralı cihazlar kargolandıysa veya girdiyse
        // hareketin içinde bu numaraları da bilelim diye buraya ekliyoruz
        public string? SerialNumber { get; set; }
        public string? EthMac { get; set; }
        public string? WlanMac { get; set; }
    }
}