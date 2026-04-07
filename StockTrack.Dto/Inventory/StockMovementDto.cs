namespace StockTrack.Dto.Inventory
{
    public class StockMovementDto
    {
        public int ProductId { get; set; } // Hangi ürün?
        public int MovementStatusId { get; set; } // Neden işlem yapılıyor? (Kargo, Kurulum vs.)
        public int Quantity { get; set; } // Kaç adet?
        public int ProductStatusId { get; set; } // Yeni mi Arızalı mı? (Sadece girişte var)
        public string? Description { get; set; } // Açıklama
    }
}