namespace StockTrack.Dto.Inventory
{
    public class InventoryAddDto
    {
        // Formdan mutlaka gelmesi gereken ve zorunlu olan alanlar (int? yerine direkt int yapıyoruz)
        public int WarehouseId { get; set; }    // Depo ID
        public int CategoryId { get; set; }     // Kategori ID
        public int ProductId { get; set; }      // Seçilen Ürün ID
        public int StockQuantity { get; set; }  // Eklenecek Stok Adeti

        // Boş gelebilecek tek alan (O yüzden string?)
        public string? Description { get; set; } // Açıklama / Notlar
    }
}