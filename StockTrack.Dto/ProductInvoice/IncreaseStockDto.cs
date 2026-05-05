namespace StockTrack.Dto.ProductInvoice
{
    public class IncreaseStockDto
    {
        public int MainRepoId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int IncreaseBy { get; set; }
        public string? Description { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }

    }
}
