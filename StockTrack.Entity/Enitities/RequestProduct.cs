namespace StockTrack.Entity.Enitities
{
    public class RequestProduct
    {
        public int RequestFormId { get; set; }
        public RequestForm RequestForm { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }

    }
}
