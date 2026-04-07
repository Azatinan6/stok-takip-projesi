using System.Collections.Generic;

namespace StockTrack.Entity.Enitities
{
    public class ProductSet : EntityBase
    {
        public string Name { get; set; } // Setin Adı (Örn: Kamera Kurulum Seti)
        public string? Description { get; set; } // Opsiyonel Açıklama

        // Bir setin içinde birden fazla ürün detayı (içeriği) olabilir
        public ICollection<ProductSetItem> ProductSetItems { get; set; }
    }
}