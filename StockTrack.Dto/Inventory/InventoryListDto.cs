namespace StockTrack.Dto.Inventory
{
    public class InventoryListDto
    {
        public int Id { get; set; }
        public string WarehouseName { get; set; } // Depo Adı
        public string CategoryName { get; set; }  // Kategori
        public string Brand { get; set; }         // Marka
        public string Model { get; set; }         // Model
        public string ProductName { get; set; }   // Ürün Adı
        public string ImageUrl { get; set; }      // Görsel Yolu
        public int StockQuantity { get; set; }    // Güncel Stok Adeti
        public int StockWarningLevel { get; set; } // Stok Uyarı Seviyesi
        public bool IsActive { get; set; }        // Aktif mi?

        // Bu ürün bir Arc Box mu? (İleride Detay ekranında lazım olacak)
        public bool IsArcBox { get; set; }
    }
}