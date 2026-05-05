using Microsoft.AspNetCore.Http;

namespace StockTrack.Dto.Inventory
{
    public class InventoryAddDto
    {
        // Dropdown'dan seçilecek ID'ler (Tanımlardan gelecek)
        public int WarehouseId { get; set; } // Depo
        public int CategoryId { get; set; }  // Kategori
        public int ProductStatusId { get; set; } // Ürün Durumu (Yeni, Kullanılmış vs.)

        // Metin girişleri
        public string Brand { get; set; } // Marka
        public string Model { get; set; } // Model
        public string ProductName { get; set; } // Ürün Adı

        // Sayısal girişler
        public int StockQuantity { get; set; } // Başlangıç Stok Adeti
        public int StockWarningLevel { get; set; } // Stok Uyarı Seviyesi

        // Ekstra bilgiler
        public string Description { get; set; } // Açıklama

        // Görsel Yükleme İşlemi için IFormFile kullanılır
        public IFormFile? ProductImage { get; set; }
    }
}