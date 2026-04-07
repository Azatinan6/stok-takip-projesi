namespace StockTrack.Dto.RequestForm
{
    public class ProductDetailDto
    {
        public string ImageUrl { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int OperationType { get; set; }
        public string? Label { get; set; }
        public bool HasConfig { get; set; } // Config girilmiş mi?
    }
}
