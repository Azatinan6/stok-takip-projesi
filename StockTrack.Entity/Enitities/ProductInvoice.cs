namespace StockTrack.Entity.Enitities
{
    public class ProductInvoice : EntityBase
    {
        public int MainRepoId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string? Description { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
    }
}
