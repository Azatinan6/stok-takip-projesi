namespace StockTrack.Entity.Enitities
{
    public class ProductSetItem : EntityBase
    {
        // Hangi Sete ait?
        public int ProductSetId { get; set; }
        public ProductSet ProductSet { get; set; }

        // İçindeki Ürün Ne?
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Bu üründen setin içinde kaç adet var? (Varsayılan 1)
        public int Quantity { get; set; } = 1;
    }
}