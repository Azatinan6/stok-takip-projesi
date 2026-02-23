namespace StockTrack.Dto.Product
{
    public class ProductStockDto
    {
        public string MainRepoName { get; set; }
        public int TotalQuantity { get; set; }
        public int DistributedQuantity { get; set; }
        public int RemainingQuantity { get; set; }
    }
}
