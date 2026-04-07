namespace StockTrack.Entity.Enitities
{
    public class ProductMainRepoLocation : EntityBase
    {
        // Hangi Ürün?
        public int ProductId { get; set; }
        public Product Product { get; set; }

        // Hangi Depoda?
        public int MainRepoLocationId { get; set; }
        public MainRepoLocation MainRepoLocation { get; set; }

        // Kaç Tane Var? (İşte PDF'teki Stok Adeti alanı burası!)
        public int Quantity { get; set; }

        // Ürün Durumu Nedir? (PDF'teki Yeni-Kullanılmış-Arızalı)
        // Bunu Tanımlar'dan çekeceğimiz için ID olarak tutuyoruz
        public int ProductStatusId { get; set; }
    }
}